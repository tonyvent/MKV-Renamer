# MKV Renamer

> **One-click tidy for movie folders.**  
> Rename the main movie, sweep Extras into place, and batch-name the rest — fast, safe, and consistent.

---

## ✨ Why MKV Renamer?

- **Speed-first workflow** — point to a folder, pick the longest file as main, click once.  
- **Consistent library** — names always match the folder (`Title (Year).mkv`) and Extras follow a neat sequence.  
- **Zero-risk moves** — never overwrites; auto-adds ` (1)`, ` (2)`, … if needed.  
- **Focused & predictable** — only touches the folder you choose; no surprises, no background services.  
- **Built for collectors & media servers** — perfect for Plex / Jellyfin / Emby style organization.

> ✅ **Privacy-friendly:** No internet access. The app only reads/writes files you select.

---

## 🚀 What it does (in seconds)

- Rename the **main movie** to match its folder (e.g., `Austin Powers Goldmember (2002).mkv`)
- Move selected files into an **`Extras`** subfolder
- **Batch-rename** extras (`Extras - 01.mkv`, `Extras - 02.mkv`, … or any prefix you choose)
- Create **new movie folders** (optionally with `Extras`) in one step
- Modern, clean UI with a deep-green accent and rounded buttons

---

## ⚡ Quick Start

### 1) Main & Extras
1. Click **Browse…** and pick a movie folder (named like `Title (Year)`).
2. Select the movie file, click **Set Selected as Main**.  
   → It’ll be renamed to **exactly** the folder name + `.mkv`.
3. *(Optional)* Check **I have extras to add**, tick the extra files, then **Rename / Move**.  
   → An `Extras` folder is created and the selected files are moved into it.

### 2) Rename Extras (batch)
1. Choose a folder with extra MKVs.
2. *(Optional)* Multi-select to rename only those; none selected = **all**.
3. Set your **Prefix** (default `Extras - `) and **Start** number, then click **Rename**.

### 3) New Movie Folder
1. Pick the **parent directory**.
2. Enter **Folder name** (e.g., `Movie Title (2025)`).
3. *(Optional)* Check **Also create Extras subfolder**, then **Create Folder**.

---

## 🧠 How it simplifies file management

- **One pass, multiple tasks:** Rename main + sweep Extras + sequence names — without bouncing between Explorer windows.  
- **Duration column** helps you spot the real movie at a glance.  
- **Predictable output** means smoother matches for Plex/Jellyfin agents.  
- **Safe by default:** If a file with the target name exists, MKV Renamer chooses `Title (Year) (1).mkv`, not overwrite.  
- **No setup required:** Portable executable; works anywhere you have write access.

> **Before**  
> ```
> Austin Powers Goldmember (2002)/
> ├─ ap_gm_final1080.mkv
> ├─ ct_t00.mkv
> └─ r_t103.mkv
> ```
> **After**  
> ```
> Austin Powers Goldmember (2002)/
> ├─ Austin Powers Goldmember (2002).mkv
> └─ Extras/
>    ├─ Extras - 01.mkv
>    └─ Extras - 02.mkv
> ```

---

## 🛡️ Safe by design

- **No overwrites** — auto-increments with ` (1)`, ` (2)`, …  
- **Only your selection** — operates strictly on the chosen folder and the files you pick.  
- **No background processes** — changes happen only when you click.

---

## 🎛️ Features

- **Duration column** in the main list to pick the real movie quickly  
- **Modern UI** (rounded buttons, spacious layout, deep-green accent)  
- **Portable** (single folder; no installer required)  
- **Windows Shell integration** for duration reading  
- **MKV-focused** (clear scope, reliable behavior)

---

## 🖼️ Screenshots

**Main & Extras**  
![Main view](Docs/screenshot-main.png)

**Rename Extras**  
![Rename Extras](Docs/screenshot-rename.png)

**New Movie Folder**  
![New Folder](Docs/screenshot-newfolder.png)

---

## 🧩 Tips for faster workflows

- **Multi-select** with `Shift` / `Ctrl` when choosing Extras to batch in one shot.  
- **Prefix once** — set your preferred Extras prefix (e.g., `Featurette - `) and hammer **Rename**.  
- **Keyboard flow:**  
  - `Tab` through inputs  
  - `Enter` to trigger the focused button  
  - `Ctrl+A` in the list to select all Extras

---

## ⚠️ Known Limitations

- Targets `.mkv` files (by design).  
- Duration comes from Windows Shell; some unusual files may show blank.  
- Operates on the **selected folder only** (no recursive subfolders).

---
