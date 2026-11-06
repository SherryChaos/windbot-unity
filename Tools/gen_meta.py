# -*- coding: utf-8 -*-
"""Create Unity .meta files for every file/folder in this UPM package.

UPM git packages are immutable: Unity does not generate .meta files for them,
so every asset must ship its own .meta or it is ignored (scripts included).

Usage:
  python Tools/gen_meta.py            # create missing metas
  python Tools/gen_meta.py --prune    # also delete orphan metas (target deleted upstream)
"""
import io, os, sys, uuid

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
SKIP_DIRS = {'.git', 'bin', 'obj', 'Library', 'Temp', '.vs', 'Logs', 'UserSettings'}

FOLDER = """fileFormatVersion: 2
guid: {guid}
folderAsset: yes
DefaultImporter:
  externalObjects: {{}}
  userData: 
  assetBundleName: 
  assetBundleVariant: 
"""

MONO = """fileFormatVersion: 2
guid: {guid}
MonoImporter:
  externalObjects: {{}}
  serializedVersion: 2
  defaultReferences: []
  executionOrder: 0
  icon: {{instanceID: 0}}
  userData: 
  assetBundleName: 
  assetBundleVariant: 
"""

ASMDEF = """fileFormatVersion: 2
guid: {guid}
AssemblyDefinitionImporter:
  externalObjects: {{}}
  userData: 
  assetBundleName: 
  assetBundleVariant: 
"""

TEXT = """fileFormatVersion: 2
guid: {guid}
TextScriptImporter:
  externalObjects: {{}}
  userData: 
  assetBundleName: 
  assetBundleVariant: 
"""

DEFAULT = """fileFormatVersion: 2
guid: {guid}
DefaultImporter:
  externalObjects: {{}}
  userData: 
  assetBundleName: 
  assetBundleVariant: 
"""


def template_for(path, is_dir):
    if is_dir:
        return FOLDER
    ext = os.path.splitext(path)[1].lower()
    if ext == '.cs':
        return MONO
    if ext == '.asmdef':
        return ASMDEF
    if ext in ('.json', '.xml'):
        return TEXT
    return DEFAULT


def prune_orphans():
    """Remove .meta files whose target no longer exists (after upstream deletions)."""
    removed = 0
    for dirpath, dirnames, filenames in os.walk(ROOT):
        dirnames[:] = [d for d in dirnames if d not in SKIP_DIRS and not d.startswith('.')]
        for name in filenames:
            if not name.endswith('.meta'):
                continue
            meta = os.path.join(dirpath, name)
            if not os.path.exists(meta[:-5]):
                os.remove(meta)
                removed += 1
    print('pruned', removed, 'orphan meta files')


def main():
    created = 0
    for dirpath, dirnames, filenames in os.walk(ROOT):
        dirnames[:] = [d for d in dirnames if d not in SKIP_DIRS and not d.startswith('.')]
        rel = os.path.relpath(dirpath, ROOT)
        rel_dir = '' if rel == '.' else rel.replace('\\', '/')

        if rel_dir:
            meta = os.path.join(os.path.dirname(dirpath), os.path.basename(dirpath) + '.meta')
            if not os.path.exists(meta):
                with io.open(meta, 'w', encoding='utf-8', newline='\n') as f:
                    f.write(FOLDER.format(guid=uuid.uuid4().hex))
                created += 1

        for name in filenames:
            if name.endswith('.meta') or name.startswith('.'):
                continue
            path = os.path.join(dirpath, name)
            meta = path + '.meta'
            if os.path.exists(meta):
                continue
            with io.open(meta, 'w', encoding='utf-8', newline='\n') as f:
                f.write(template_for(path, False).format(guid=uuid.uuid4().hex))
            created += 1

    print('created', created, 'meta files')


if __name__ == '__main__':
    if '--prune' in sys.argv:
        prune_orphans()
    main()
