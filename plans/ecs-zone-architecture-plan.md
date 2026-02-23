# ECS-Authoritative Zone Architecture Implementation Plan  
  
## Overview  
This plan outlines the implementation of an ECS-authoritative zone system for the Bluelock plugin. 
  
## Target Directory Structure  
  
```  
Bluelock/Systems/ECS/  
├── Components/  
│   ├── ZoneComponent.cs  
│   ├── PlayerZoneState.cs  
│   └── ZoneTransitionEvent.cs  
├── Utilities/  
│   └── ZoneHashUtility.cs  
└── Systems/  
    ├── ZoneBootstrapSystem.cs  
    ├── ZoneDetectionSystem.cs  
    ├── ZoneTemplateLifecycleSystem.cs  
    ├── FlowExecutionSystem.cs  
    └── ZoneVisualSystem.cs  
``` 
## ECS Components  
  
### 1.1 ZoneComponent.cs  
- Purpose: Stores zone data for spatial detection  
- Fields:  
  - ZoneHash: int - Hash of zone ID for efficient lookup  
  - ZoneId: FixedString32Bytes - Original zone ID  
  - Center: float3 - Zone center position  
  - EntryRadius: float - Entry transition radius  
  - ExitRadius: float - Exit transition radius  
  - FlowId: FixedString32Bytes - Flow to execute on transitions  
  
### 1.2 PlayerZoneState.cs  
- Purpose: Tracks player current zone membership  
- Fields:  
  - CurrentZoneHash: int - Hash of zone player is in  
  - LastUpdateTime: double - Last position check timestamp  
  
### 1.3 ZoneTransitionEvent.cs  
- Purpose: Event component for zone enter/exit  
- Fields:  
  - PlayerEntity: Entity - The player entity  
  - FromZoneHash: int - Previous zone  
  - ToZoneHash: int - New zone  
  - TransitionType: int - 0=Enter, 1=Exit 
## ECS Systems  
  
### ZoneBootstrapSystem.cs  
- Purpose: Convert ZoneDefinitions to ECS components at startup  
- Logic:  
  1. Iterate through ZoneConfigService.GetAllZones  
  2. Create Entity for each zone via EntityManager  
  3. Add ZoneComponent with data from ZoneDefinition  
  
### ZoneDetectionSystem.cs  
- Purpose: Detect player positions with entry/exit radius logic  
- Logic:  
  1. Query all players with PlayerZoneState component  
  2. Get player position from LocalTransform  
  3. Iterate through all ZoneComponent entities  
  4. Calculate distance from player to zone center  
  5. Check EntryRadius for enter, ExitRadius for exit  
  
### ZoneTemplateLifecycleSystem.cs  
- Purpose: ECS-driven template spawn/clear on zone transitions  
- Backward Compatibility: Delegate to existing ZoneTemplateService  
  
### FlowExecutionSystem.cs  
- Purpose: Execute flow logic on zone transitions  
- Depends on: Core FlowService  
  
### ZoneVisualSystem.cs  
- Purpose: Manage visual entities (borders, glow tiles) 
