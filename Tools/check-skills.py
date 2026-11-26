#!/usr/bin/env python3
"""Validate the agent skills in this repository.

Checks , for every skill directory :
  - SKILL.md exists and starts with a YAML frontmatter block
  - the frontmatter has a name and a description
  - name matches the directory name and the agentskills.io character rules
  - every relative markdown link in SKILL.md resolves
  - the UPM copy under Samples~/AgentSkill matches skills/uniink ( .meta aside )

Usage: Tools/check-skills.py
"""
import pathlib
import re
import sys

ROOT = pathlib.Path(__file__).resolve().parent.parent

SKILLS = [
    ROOT / 'skills/uniink',
    ROOT / '.github/skills/uniink-repo',
]

NAME_RULE = re.compile(r'^[a-z0-9]+(?:-[a-z0-9]+)*$')
LINK_RULE = re.compile(r'\]\(([^)\s]+\.md)\)')
UPM_COPY = ROOT / 'Unity/Packages/com.arc.uniink/Samples~/AgentSkill'

errors: list[str] = []


def fail(message: str) -> None:
    errors.append(message)
    print(f'  FAIL {message}')


def read_frontmatter(skill_md: pathlib.Path) -> dict[str, str]:
    text = skill_md.read_text(encoding='utf-8')

    if not text.startswith('---\n'):
        fail(f'{skill_md}: does not start with a frontmatter block')
        return {}

    end = text.find('\n---', 4)
    if end < 0:
        fail(f'{skill_md}: frontmatter is not closed')
        return {}

    fields: dict[str, str] = {}
    for line in text[4:end].splitlines():
        if ':' not in line or line.startswith(' '):
            continue
        key, value = line.split(':', 1)
        fields[key.strip()] = value.strip().strip('"')

    return fields


for skill in SKILLS:
    print(f'checking {skill.relative_to(ROOT)}')
    skill_md = skill / 'SKILL.md'

    if not skill_md.is_file():
        fail(f'{skill_md}: SKILL.md is missing')
        continue

    fields = read_frontmatter(skill_md)
    name = fields.get('name', '')
    description = fields.get('description', '')

    if not name:
        fail(f'{skill_md}: name is missing')
    elif name != skill.name:
        fail(f'{skill_md}: name "{name}" does not match the directory "{skill.name}"')
    elif not NAME_RULE.match(name) or len(name) > 64:
        fail(f'{skill_md}: name "{name}" is not lowercase-hyphenated ( max 64 )')

    if not description:
        fail(f'{skill_md}: description is missing')
    elif len(description) > 1024:
        fail(f'{skill_md}: description is {len(description)} characters , the limit is 1024')

    text = skill_md.read_text(encoding='utf-8')
    for target in sorted(set(LINK_RULE.findall(text))):
        if target.startswith(('http://', 'https://')):
            continue
        if not (skill / target).is_file():
            fail(f'{skill_md}: link target "{target}" does not exist')

# the Unity package carries a copy of the consumer skill
print('checking the UPM copy of skills/uniink')
source = SKILLS[0]

if not UPM_COPY.is_dir():
    fail(f'{UPM_COPY}: the Agent Skill sample is missing ( run Tools/sync-unity-package.sh )')
else:
    def files(folder: pathlib.Path) -> set[str]:
        return {
            str(p.relative_to(folder))
            for p in folder.rglob('*')
            if p.is_file() and p.suffix != '.meta'
        }

    source_files = files(source)
    copy_files = files(UPM_COPY)

    for missing in sorted(source_files - copy_files):
        fail(f'{UPM_COPY}: missing {missing}')
    for extra in sorted(copy_files - source_files):
        fail(f'{UPM_COPY}: unexpected {extra}')

    for name in sorted(source_files & copy_files):
        if (source / name).read_bytes() != (UPM_COPY / name).read_bytes():
            fail(f'{UPM_COPY}/{name}: differs from the source')

if errors:
    print(f'\n{len(errors)} problem(s)')
    sys.exit(1)

print('\nall skills are valid')
