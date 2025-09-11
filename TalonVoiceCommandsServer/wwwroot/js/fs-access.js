// JS module for directory picking and reading files (File System Access API + webkitdirectory fallback)
export async function pickDirectoryFiles() {
  if (!window.showDirectoryPicker) {
    throw new Error('showDirectoryPicker not available');
  }

  const dirHandle = await window.showDirectoryPicker();
  const files = [];

  async function recurse(handle, path) {
    for await (const [name, entry] of handle.entries()) {
      const entryPath = path ? `${path}/${name}` : name;
      if (entry.kind === 'file') {
        const file = await entry.getFile();
        const text = await file.text();
        files.push({ name: entryPath, text });
      } else if (entry.kind === 'directory') {
        await recurse(entry, entryPath);
      }
    }
  }

  await recurse(dirHandle, '');
  return files; // [{ name, text }, ...]
}

// Fallback: create a hidden input with webkitdirectory to let user pick a folder
export async function readDirectoryViaInput() {
  return new Promise((resolve) => {
    const input = document.createElement('input');
    input.type = 'file';
    input.multiple = true;
    input.webkitdirectory = true; // supported by Chromium and some other browsers
    input.style.display = 'none';
    document.body.appendChild(input);

    input.addEventListener('change', async () => {
      const results = [];
      const files = Array.from(input.files || []);
      for (const f of files) {
        try {
          const text = await f.text();
          // webkitRelativePath includes the relative path from the chosen folder in supporting browsers
          const name = (f.webkitRelativePath && f.webkitRelativePath.length > 0) ? f.webkitRelativePath : f.name;
          results.push({ name, text });
        } catch (e) {
          // skip file on read error
          console.warn('Error reading file', f.name, e);
        }
      }
      document.body.removeChild(input);
      resolve(results);
    }, { once: true });

    // Trigger the file picker
    input.click();
  });
}

// Keep the last picked directory handle in module scope for subsequent calls
let _lastPickedDirHandle = null;

// Let the user pick the Talon 'user' folder and return immediate subdirectories
// with a small count of .talon files (recursive count) so the UI can present repos.
export async function pickUserFolderAndListRepos() {
  if (!window.showDirectoryPicker) throw new Error('showDirectoryPicker not available');
  _lastPickedDirHandle = await window.showDirectoryPicker();
  const repos = [];

  for await (const [name, entry] of _lastPickedDirHandle.entries()) {
    if (entry.kind === 'directory') {
      // count .talon files recursively inside this repo (limit depth/items to avoid long runs)
      let count = 0;
      async function recurse(handle) {
        for await (const [n, e] of handle.entries()) {
          try {
            if (e.kind === 'file') {
              if (n.toLowerCase().endsWith('.talon')) count++;
              if (count > 1000) return; // safety cap
            } else if (e.kind === 'directory') {
              await recurse(e);
              if (count > 1000) return;
            }
          } catch (err) {
            // ignore inaccessible entries
            console.warn('recurse entry error', err);
          }
        }
      }
      try { await recurse(entry); } catch (e) { console.warn('count error', e); }
      repos.push({ name, talonFileCount: count });
    }
  }

  return { rootName: _lastPickedDirHandle.name || '', repos };
}

// Given a list of repo names (immediate subfolders of the previously-picked root),
// return all .talon files under those repos as [{ name: relativePath, text }...]
export async function getFilesForRepos(repoNames) {
  if (!_lastPickedDirHandle) throw new Error('No folder picked');
  const results = [];

  for await (const [name, entry] of _lastPickedDirHandle.entries()) {
    if (entry.kind === 'directory' && repoNames.includes(name)) {
      async function recurse(handle, path) {
        for await (const [n, e] of handle.entries()) {
          try {
            const entryPath = path ? `${path}/${n}` : n;
            if (e.kind === 'file') {
              if (n.toLowerCase().endsWith('.talon')) {
                const file = await e.getFile();
                const text = await file.text();
                results.push({ name: `${name}/${entryPath}`, text });
              }
            } else if (e.kind === 'directory') {
              await recurse(e, entryPath);
            }
          } catch (err) {
            console.warn('read file error', err);
          }
        }
      }
      try { await recurse(entry, ''); } catch (e) { console.warn('recurse read error', e); }
    }
  }

  return results;
}
