#!/usr/bin/env python3
"""
API Report Generator for VAutomationCore
Scans source files and generates a debug API report.
Run from repo root: python scripts/generate_api_report.py
"""

import os
import re
import json
from pathlib import Path
from collections import defaultdict
from datetime import datetime

# Configuration
ROOT_DIR = Path(__file__).parent.parent
OUTPUT_FILE = ROOT_DIR / "Bluelock" / "config" / "debug_api_report.txt"

# Directories to scan
SCAN_DIRS = [
    ROOT_DIR / "Bluelock",
    ROOT_DIR / "Core",
    ROOT_DIR / "Patches",
    ROOT_DIR / "CycleBorn",
]

# File patterns to include
INCLUDE_EXTENSIONS = {".cs"}

# Patterns to extract
HARMONY_PATCH_nPATTERN = re.compile(
    r'\[HarmonyPatch\s*\(\s*typeof\s*\(\s*(\w+)\s*\)\s*(?:,\s*"([^"]+)")?\s*\)\]'
)
HARMONY_METHOD_PATTERN = re.compile(
    r'\[Harmony(?:Prefix|Postfix|Transpiler)\s*\(\s*(?:typeof\s*\(\s*(\w+)\s*\)\s*,\s*)?nameof\s*\(\s*(\w+)\s*\(\s*\)\s*\)\s*\)'
)
NAMESPACE_PATTERN = re.compile(r'^namespace\s+([\w.]+)', re.MULTILINE)
CLASS_PATTERN = re.compile(r'(?:public|internal|private|protected)?\s*(?:static|sealed)?\s*(?:class|struct|enum|interface)\s+(\w+)')
METHOD_PATTERN = re.compile(
    r'(?:public|private|protected|internal|static|virtual|override|abstract|sealed)?\s*'
    r'(?:async\s+)?(?:\w+(?:<[^>]+>)?(?:\?)?(?:\[\])?)\s+'
    r'(\w+)\s*\(([^)]*)\)'
)
EVENT_PATTERN = re.compile(r'public\s+(?:static\s+)?event\s+(\w+(?:<[^>]+>)?)\s+(\w+);')
CONFIG_ENTRY_PATTERN = re.compile(
    r'public\s+static\s+ConfigEntry<(\w+)>\s+(\w+)'
)

class APIExtractor:
    def __init__(self):
        self.types = {}
        self.methods = defaultdict(list)
        self.patches = []
        self.namespaces = set()
        self.events = []
        self.config_entries = []
        
    def scan_file(self, filepath: Path):
        """Scan a single C# file for API signatures."""
        try:
            content = filepath.read_text(encoding='utf-8', errors='ignore')
        except Exception as e:
            print(f"Error reading {filepath}: {e}")
            return
            
        # Extract namespace
        ns_match = NAMESPACE_PATTERN.search(content)
        namespace = ns_match.group(1) if ns_match else "global"
        self.namespaces.add(namespace)
        
        # Extract classes/structs
        for class_match in CLASS_PATTERN.finditer(content):
            class_name = class_match.group(1)
            full_name = f"{namespace}.{class_name}"
            
            if full_name not in self.types:
                self.types[full_name] = {
                    'name': class_name,
                    'namespace': namespace,
                    'kind': 'class',
                    'file': str(filepath.relative_to(ROOT_DIR))
                }
            
            # Check for HarmonyPatch attribute on class
            class_start = class_match.start()
            class_content = content[class_start:class_start+500]
            if '[HarmonyPatch]' in class_content:
                self.types[full_name]['is_patch'] = True
                
            # Extract methods in this class
            method_matches = METHOD_PATTERN.finditer(content)
            for m in method_matches:
                method_name = m.group(1)
                params = m.group(2).strip()
                
                # Skip constructors, operators, properties
                if method_name in ('get_', 'set_', 'add_', 'remove_', 'op_'):
                    continue
                if method_name == class_name:  # Constructor
                    continue
                    
                full_method_sig = f"{method_name}({params})"
                if full_method_sig not in self.methods[full_name]:
                    self.methods[full_name].append({
                        'name': method_name,
                        'parameters': params,
                        'signature': full_method_sig
                    })
        
        # Extract Harmony patches - look for HarmonyPatch attribute anywhere
        # Pattern 1: [HarmonyPatch(typeof(X), nameof(Y))]
        patch_pattern_1 = re.compile(
            r'\[HarmonyPatch\s*\(\s*typeof\s*\(\s*(\w+)\s*\)\s*(?:,\s*nameof\s*\(\s*(\w+)\s*\(\s*\)\s*\)\s*)?\)\s*\]'
        )
        for m in patch_pattern_1.finditer(content):
            target_class = m.group(1)
            target_method = m.group(2) if m.group(2) else "OnUpdate"  # Default to OnUpdate if not specified
            self.patches.append({
                'target_class': target_class,
                'target_method': target_method,
                'file': str(filepath.relative_to(ROOT_DIR))
            })
        
        # Pattern 2: [HarmonyPatch(typeof(X), "Y")]
        patch_pattern_2 = re.compile(
            r'\[HarmonyPatch\s*\(\s*typeof\s*\(\s*(\w+)\s*\)\s*,\s*"([^"]+)"\s*\)\]'
        )
        for m in patch_pattern_2.finditer(content):
            self.patches.append({
                'target_class': m.group(1),
                'target_method': m.group(2),
                'file': str(filepath.relative_to(ROOT_DIR))
            })
        
        # Pattern 3: Class-level [HarmonyPatch] with internal HarmonyPatchClass
        class_patch_pattern = re.compile(
            r'\[HarmonyPatch\]\s*\n\s*internal\s+(?:static\s+)?class\s+(\w+)',
            re.MULTILINE
        )
        for m in class_patch_pattern.finditer(content):
            # Find method patches inside this class
            class_name = m.group(1)
            class_start = m.start()
            # Get a chunk after the class definition to find methods
            class_chunk = content[class_start:class_start+3000]
            
            # Find HarmonyPrefix/Postfix methods
            method_pattern = re.compile(
                r'(?:public|internal|private|protected)\s+(?:static\s+)?(?:void|bool)\s+(\w+)\s*\([^)]*\)\s*\{'
            )
            for method_match in method_pattern.finditer(class_chunk):
                method_name = method_match.group(1)
                self.patches.append({
                    'target_class': f"(class {class_name})",
                    'target_method': method_name,
                    'patch_class': class_name,
                    'file': str(filepath.relative_to(ROOT_DIR))
                })
        
        # Extract events
        for event_match in EVENT_PATTERN.finditer(content):
            event_type = event_match.group(1)
            event_name = event_match.group(2)
            self.events.append({
                'type': event_type,
                'name': event_name,
                'namespace': namespace,
                'file': str(filepath.relative_to(ROOT_DIR))
            })
        
        # Extract ConfigEntry
        for config_match in CONFIG_ENTRY_PATTERN.finditer(content):
            config_type = config_match.group(1)
            config_name = config_match.group(2)
            self.config_entries.append({
                'type': config_type,
                'name': config_name,
                'namespace': namespace,
                'file': str(filepath.relative_to(ROOT_DIR))
            })
    
    def scan_all(self):
        """Scan all relevant directories."""
        for scan_dir in SCAN_DIRS:
            if not scan_dir.exists():
                print(f"Warning: Directory not found: {scan_dir}")
                continue
                
            print(f"Scanning {scan_dir}...")
            for root, dirs, files in os.walk(scan_dir):
                for file in files:
                    if Path(file).suffix in INCLUDE_EXTENSIONS:
                        filepath = Path(root) / file
                        self.scan_file(filepath)
    
    def generate_report(self) -> str:
        """Generate the API report."""
        lines = []
        lines.append("=" * 70)
        lines.append(" API SIGNATURE REPORT")
        lines.append(f" Generated: {datetime.now().isoformat()}")
        lines.append("=" * 70)
        lines.append("")
        
        # Summary stats
        lines.append("[SUMMARY]")
        lines.append(f" Total Namespaces: {len(self.namespaces)}")
        lines.append(f" Total Types: {len(self.types)}")
        lines.append(f" Total Methods: {sum(len(v) for v in self.methods.values())}")
        lines.append(f" Total Harmony Patches: {len(self.patches)}")
        lines.append(f" Total Events: {len(self.events)}")
        lines.append(f" Total Config Entries: {len(self.config_entries)}")
        lines.append("")
        
        # Namespaces
        lines.append("-" * 70)
        lines.append("[NAMESPACES]")
        lines.append("-" * 70)
        for ns in sorted(self.namespaces):
            lines.append(f"  {ns}")
        lines.append("")
        
        # Types
        lines.append("-" * 70)
        lines.append("[TYPES]")
        lines.append("-" * 70)
        
        # Group types by namespace
        by_namespace = defaultdict(list)
        for full_name, type_info in self.types.items():
            by_namespace[type_info['namespace']].append((full_name, type_info))
        
        for ns in sorted(by_namespace.keys()):
            lines.append(f"\n  // {ns}")
            for full_name, type_info in sorted(by_namespace[ns]):
                kind = type_info.get('kind', 'type')
                is_patch = type_info.get('is_patch', False)
                marker = " [PATCH]" if is_patch else ""
                lines.append(f"  {full_name}{marker}")
                lines.append(f"    File: {type_info['file']}")
        lines.append("")
        
        # Harmony Patches
        lines.append("-" * 70)
        lines.append("[HARMONY PATCHES]")
        lines.append("-" * 70)
        
        if self.patches:
            by_target = defaultdict(list)
            for p in self.patches:
                key = f"{p.get('target_class', 'Unknown')}.{p.get('target_method', 'Unknown')}"
                by_target[key].append(p)
            
            for target, patches in sorted(by_target.items()):
                lines.append(f"\n  Target: {target}")
                for p in patches:
                    if 'patch_method' in p:
                        lines.append(f"    -> {p.get('patch_class', '')}.{p['patch_method']}")
                    lines.append(f"    File: {p['file']}")
        else:
            lines.append("  (No patches found)")
        lines.append("")
        
        # Config Entries
        lines.append("-" * 70)
        lines.append("[CONFIG ENTRIES]")
        lines.append("-" * 70)
        
        if self.config_entries:
            for cfg in sorted(self.config_entries, key=lambda x: x['namespace']):
                lines.append(f"  {cfg['namespace']}.{cfg['type']} {cfg['name']}")
                lines.append(f"    File: {cfg['file']}")
        else:
            lines.append("  (No config entries found)")
        lines.append("")
        
        # Events
        lines.append("-" * 70)
        lines.append("[EVENTS]")
        lines.append("-" * 70)
        
        if self.events:
            for evt in sorted(self.events, key=lambda x: x['namespace']):
                lines.append(f"  event {evt['type']} {evt['name']}")
                lines.append(f"    File: {evt['file']}")
        else:
            lines.append("  (No events found)")
        lines.append("")
        
        # Critical Checks
        lines.append("=" * 70)
        lines.append("[CRITICAL CHECKS]")
        lines.append("=" * 70)
        
        # Check for unsafe using statements
        lines.append("\n[UNSAFE PATTERNS]")
        
        # Scan for using var with NativeArray
        unsafe_patterns = [
            (r'using\s+var\s+\w+\s*=.*?\.To\w+Array\w*\(Allocator\.Temp\)', 
             "using var with NativeArray - unsafe in IL2CPP"),
            (r'using\s+var\s+\w+\s*=.*?\.ToComponentDataArray\w*\(Allocator\.Temp\)',
             "using var with NativeArray - unsafe in IL2CPP"),
        ]
        
        unsafe_found = []
        for scan_dir in SCAN_DIRS:
            if not scan_dir.exists():
                continue
            for root, dirs, files in os.walk(scan_dir):
                for file in files:
                    if not file.endswith('.cs'):
                        continue
                    filepath = Path(root) / file
                    try:
                        content = filepath.read_text(encoding='utf-8', errors='ignore')
                        for pattern, desc in unsafe_patterns:
                            matches = re.finditer(pattern, content)
                            for m in matches:
                                unsafe_found.append({
                                    'file': str(filepath.relative_to(ROOT_DIR)),
                                    'pattern': desc,
                                    'line': content[:m.start()].count('\n') + 1
                                })
                    except:
                        pass
        
        if unsafe_found:
            for u in unsafe_found:
                lines.append(f"  ⚠️  {u['file']}:{u['line']}")
                lines.append(f"      {u['pattern']}")
        else:
            lines.append("  ✅ No unsafe using patterns found")
        
        lines.append("")
        
        # Patch loading check
        lines.append("\n[PATCH LOADING STATUS]")
        
        # Check if Plugin.cs calls PatchAll
        plugin_file = ROOT_DIR / "Bluelock" / "Plugin.cs"
        if plugin_file.exists():
            content = plugin_file.read_text(encoding='utf-8', errors='ignore')
            if 'PatchAll' in content:
                lines.append("  ✅ PatchAll found in Plugin.cs")
                
                # Check for broken patterns
                if 'typeof(Patches)' in content:
                    lines.append("  ⚠️  WARNING: typeof(Patches) - this type may not exist")
                if 'GetType' in content and 'Patch' in content:
                    lines.append("  ⚠️  WARNING: Dynamic type lookup for patches - may fail")
            else:
                lines.append("  ❌ PatchAll NOT found in Plugin.cs - patches will NOT load!")
        else:
            lines.append("  ⚠️  Plugin.cs not found")
        
        lines.append("")
        
        # Config file paths check
        lines.append("\n[CONFIG PATH VALIDATION]")
        
        config_issues = []
        config_patterns = [
            (r'Path\.Combine\s*\(\s*Paths\.ConfigPath\s*,\s*"([^"]+)"', 'Hardcoded config path'),
        ]
        
        for scan_dir in SCAN_DIRS:
            if not scan_dir.exists():
                continue
            for root, dirs, files in os.walk(scan_dir):
                for file in files:
                    if not file.endswith('.cs'):
                        continue
                    filepath = Path(root) / file
                    try:
                        content = filepath.read_text(encoding='utf-8', errors='ignore')
                        for pattern, desc in config_patterns:
                            matches = re.finditer(pattern, content)
                            for m in matches:
                                path = m.group(1)
                                # Check for wrong paths
                                if 'CycleBorn' in path and 'Cycleborn' not in path:
                                    config_issues.append({
                                        'file': str(filepath.relative_to(ROOT_DIR)),
                                        'issue': f"Wrong path: {path}",
                                        'line': content[:m.start()].count('\n') + 1
                                    })
                    except:
                        pass
        
        if config_issues:
            for c in config_issues:
                lines.append(f"  ⚠️  {c['file']}:{c['line']}")
                lines.append(f"      {c['issue']}")
        else:
            lines.append("  ✅ No obvious config path issues")
        
        lines.append("")
        
        # Methods by class
        lines.append("=" * 70)
        lines.append("[METHODS BY CLASS]")
        lines.append("=" * 70)
        
        for type_name, methods in sorted(self.methods.items()):
            if methods:
                lines.append(f"\n  {type_name}")
                for m in methods:
                    params = m['parameters'] if m['parameters'] else ""
                    lines.append(f"    {m['name']}({params})")
        
        lines.append("")
        lines.append("=" * 70)
        lines.append(" END OF REPORT")
        lines.append("=" * 70)
        
        return "\n".join(lines)

def main():
    print("VAutomationCore API Report Generator")
    print("=" * 50)
    
    extractor = APIExtractor()
    extractor.scan_all()
    
    report = extractor.generate_report()
    
    # Ensure output directory exists
    OUTPUT_FILE.parent.mkdir(parents=True, exist_ok=True)
    
    # Write report
    OUTPUT_FILE.write_text(report, encoding='utf-8')
    print(f"\n✅ Report written to: {OUTPUT_FILE}")
    
    # Also print summary
    print("\n[Summary]")
    print(f"  Namespaces: {len(extractor.namespaces)}")
    print(f"  Types: {len(extractor.types)}")
    print(f"  Harmony Patches: {len(extractor.patches)}")
    print(f"  Events: {len(extractor.events)}")
    print(f"  Config Entries: {len(extractor.config_entries)}")

if __name__ == "__main__":
    main()
