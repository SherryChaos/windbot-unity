# -*- coding: utf-8 -*-
"""Create Unity .meta files for every file/folder in this UPM package.

UPM git packages are immutable: Unity does not generate .meta files for them,
so every asset must ship its own .meta or it is ignored (scripts included).
"""
import io, os, uuid

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


def main():
    created = 0
    for dirpath, dirnames, filenames in os.walk(ROOT):
        dirnames[:] = [d for d in dirnames if d not in SKIP_DIRS and not d.startswith('.')]
        rel = os.path.relpath(dirpath, ROOT)
        if rel == '.':
            rel_dir = ''
        else:
            rel_dir = rel.replace('\\', '/')

        # folder meta (skip repo root itself)
        if rel_dir:
            meta = os.path.join(dirpath, os.path.basename(dirpath) + '.meta')
            # Unity puts the meta next to the folder, i.e. parent/<folder>.meta
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
    main()
