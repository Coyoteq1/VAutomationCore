# Security Incident Playbook

This playbook defines the minimum response for private or sensitive data leaks.

## 1) Prevent

- Enforce organization-wide 2FA for members, outside collaborators, and billing managers.
- Apply least privilege for repository permissions, tokens, and GitHub App scopes.
- Restrict high-risk actions where possible: public visibility changes, forking, deletion/transfer, and uncontrolled repository creation.
- Enable secret scanning and push protection at organization level, including custom patterns for internal token formats.
- Protect default branches with required reviews, required checks, and controlled bypass.

## 2) Detect

- Monitor secret scanning alerts and route high-severity alerts to a security response channel.
- Review organization audit logs for suspicious events (permission changes, token creation/use, visibility changes, branch protection bypass).
- Declare an incident immediately when sensitive data exposure is confirmed or strongly suspected.

## 3) Mitigate

- Contain quickly: restrict repository access, disable compromised accounts, and remove unauthorized collaborators.
- Revoke and rotate exposed credentials (PATs, API keys, cloud secrets, signing keys) immediately.
- Remove sensitive data from Git history using history rewrite tooling; force-push cleaned history and coordinate local clone cleanup.
- If direct owner coordination is blocked, escalate to GitHub Support and legal channels as required.

## 4) Recovery

- Verify clean state: no active leaked credentials, no unresolved critical alerts, and no unauthorized access paths.
- Restore normal operations with hardened controls and monitoring in place.
- Complete post-incident review with timeline, root cause, impact, and corrective actions with owners and due dates.
- Update training, detection rules, and this playbook based on lessons learned.

## Reporting a Security Issue

Report vulnerabilities privately to **Ahmadtllal1@gmail.com** with:

- Description of the issue
- Reproduction steps
- Potential impact
- Suggested remediation (if available)
