# Panduan Implementasi Sistem Visual Novel (Unity Editor)

Dokumen ini berisi tutorial langkah demi langkah untuk menyusun dan mengimplementasikan sistem Visual Novel yang telah dibuat menggunakan skrip C# ke dalam Unity Editor.

## 1. Membuat Data Karakter (Character Data)

Kita membutuhkan data untuk setiap karakter agar sistem tahu nama dan ekspresi (*sprite*) yang akan ditampilkan.

**Langkah-langkah:**
1. Di jendela **Project**, klik kanan pada folder tujuan (misalnya `Assets/Resources/Characters`).
2. Pilih **Create > Visual Novel > Character Data**.
3. Beri nama file baru tersebut, contoh: `Char_Rifqi`.

**Contoh Setup di Inspector:**
- **Character Name:** Rifqi
- **Neutral Portrait:** Masukkan *sprite* Rifqi dengan wajah normal.
- **Happy Portrait:** Masukkan *sprite* Rifqi yang sedang tersenyum.
- *(Lengkapi sprite emosi lainnya sesuai kebutuhan Anda).*

---

## 2. Membuat Data Dialog (Dialogue Data)

Data ini berisi skenario alur cerita visual novel.

**Langkah-langkah:**
1. Klik kanan di folder penyimpanan cerita (misalnya `Assets/Resources/Dialogues`).
2. Pilih **Create > Visual Novel > Dialogue Data**.
3. Beri nama file baru tersebut, contoh: `Chapter1_Intro`.
4. Di **Inspector**, pada properti **Lines**, klik tombol `+` untuk menambah baris dialog.

**Contoh Setup 1 Baris Dialog:**
- **Speaker:** Tarik file `Char_Rifqi` yang dibuat di langkah 1 ke kotak ini.
- **Text:** "Akhirnya kita berhasil menyelesaikannya."
- **Emotion:** Pilih `Happy`.
- **Position:** Pilih `Left`.
- **Background Override:** (Biarkan kosong jika tidak ada perpindahan layar background).

---

## 3. Menyusun Struktur UI di Scene (Tanpa Perlu Pindah Scene!)

**Sistem ini didesain sebagai UI Overlay (Tumpukan UI).** Artinya, Anda **TIDAK PERLU** pindah ke *Scene* baru (seperti `DialogueScene`). Cukup pasang seluruh Canvas ini di dalam *Scene Gameplay* utama Anda. 

Saat dipanggil, panel UI dialog akan otomatis muncul menutupi gameplay, dan saat percakapan selesai, panel akan hilang dan pemain bisa langsung lanjut bermain tanpa *loading screen*.

**Struktur Hierarki Rekomendasi (Masukkan ke dalam Scene Gameplay):**
```text
Canvas
 ├── Background (Image) -> *Tambahkan skrip BackgroundFader.cs di sini*
 │    └── FadeOverlay (Image warna hitam transparan untuk efek gelap saat pindah bg)
 ├── LeftPortraitRoot (Image) -> *Tambahkan skrip PortraitSlot.cs di sini*
 ├── CenterPortraitRoot (Image) -> *Tambahkan skrip PortraitSlot.cs di sini*
 ├── RightPortraitRoot (Image) -> *Tambahkan skrip PortraitSlot.cs di sini*
 └── DialoguePanel (GameObject Panel biasa)
      ├── SpeakerText (TextMeshProUGUI)
      └── DialogueText (TextMeshProUGUI)
```

**Konfigurasi Object UI:**
- **PortraitSlot.cs:**
  - Klik *LeftPortraitRoot*.
  - *Portrait Image*: Tarik komponen Image dari dirinya sendiri.
  - *Slot Position*: Pilih posisi `Left`. 
  - (Ulangi hal yang sama untuk *Center* dan *Right*).
- **BackgroundFader.cs:**
  - Klik objek *Background*.
  - *Background Image*: Tarik komponen Image dari dirinya sendiri.
  - *Fade Overlay*: Tarik objek *FadeOverlay* (anak dari Background).

---

## 4. Menghubungkan ke DialogueManagerCS

Otak dari seluruh sistem narasi visual novel adalah skrip `DialogueManagerCS`. Anda disarankan meletakkannya pada *GameObject* kosong bernama `Managers`.

**Langkah-langkah Setup:**
1. Buat *GameObject* kosong `DialogueManager`.
2. Tambahkan komponen skrip `DialogueManagerCS`.
3. Di tab **Inspector** skrip tersebut, Anda akan mendapati banyak slot kosong. Masukkan referensi objek UI yang sudah disusun di Langkah 3:
   - **Speaker Name Text:** Tarik objek *SpeakerText*.
   - **Dialogue Text:** Tarik objek *DialogueText*.
   - **Dialogue Panel:** Tarik objek *DialoguePanel*.
   - **Left Slot:** Tarik objek *LeftPortraitRoot*.
   - **Center Slot:** Tarik objek *CenterPortraitRoot*.
   - **Right Slot:** Tarik objek *RightPortraitRoot*.
   - **Background Fader:** Tarik objek *Background*.

---

## 5. Cara Menjalankan Percakapan Chapter (Contoh Penerapan)

Setelah semua disiapkan, Anda tinggal merujuk *method* di skrip `DialogueManagerCS` bernama `PlayDialogue()`. Skrip `ChapterManager.cs` yang dibuat tadi adalah contohnya.

**Uji Coba dengan ChapterManager:**
1. Buat Game Object kosong `ChapterManager`.
2. Tambahkan skrip `ChapterManager.cs`.
3. Di Inspector, isi kolom **Chapter Intro Data** dengan file `Chapter1_Intro` yang dibuat di Langkah 2.
4. Isi kolom **Dialogue Manager** dengan objek `DialogueManager` yang telah dihubungkan di Langkah 4.
5. Jalankan (*Play*) gamenya. Dialog akan langsung muncul secara otomatis di layar Anda.

---

## 6. Menerapkan Sistem Memory Shard (Koleksi Memori Cerita)

Sistem *Memory Shard* digunakan ketika pemain memecahkan *puzzle* atau mendapatkan item tertentu, kemudian pemain dapat menonton dialog "kepingan memori" tersebut dari dalam UI Menu.

### A. Membuat Aset Data Memory Shard
1. Di jendela **Project**, klik kanan (misal di folder `Assets/Resources/MemoryShards`).
2. Pilih **Create > Visual Novel > Memory Shard Data**.
3. Beri nama file, contoh: `Shard_Liontin_Ibu`.
4. Di **Inspector**, atur rinciannya:
   - **Shard ID:** `shard_liontin_01` *(ID unik, wajib diisi dan beda dengan shard lain)*.
   - **Title:** Liontin Pemberian Ibu.
   - **Thumbnail:** Masukkan Sprite gambar item liontinnya.
   - **Dialogue Data:** Tarik file *Dialogue Data* (`VNDialogueData`) yang berisi cerita spesifik untuk item ini (Buat terlebih dahulu file dialognya seperti pada Langkah 2).
   - **Is Unlocked:** Biarkan **tidak dicentang (false)** karena item ini akan dikunci di awal permainan.

### B. Memasang Memory Shard Manager
1. Buat *GameObject* kosong bernama `MemoryShardManager` di dalam *Scene*.
2. Tambahkan skrip `MemoryShardManager.cs`.
3. Di **Inspector**, perhatikan list **All Shards**:
   - Tekan ikon `+` dan masukkan file `Shard_Liontin_Ibu` yang baru saja Anda buat.
   - Ulangi untuk semua *shard* lain yang ada di dalam game.
4. Pada kolom **Dialogue Manager**, hubungkan (tarik) *GameObject* `DialogueManager` yang sudah Anda setup di Langkah 4.

### C. Cara Membuka (Unlock) Shard dari Puzzle
Saat *player* mengambil barang atau menyelesaikan puzzle, Anda perlu memberi perintah kepada skrip untuk membuka kunci *shard*.
1. Pada skrip puzzle Anda (contohnya `SisikPuzzleManager.cs` atau `BarangTrigger.cs`), pastikan Anda punya referensi ke `MemoryShardManager`.
2. Panggil baris kode ini tepat ketika puzzle selesai/item diambil:
   ```csharp
   // Contoh cara memanggilnya
   memoryShardManager.UnlockShard("shard_liontin_01");
   ```
3. Skrip akan otomatis mengubah status shard dengan ID tersebut menjadi *terbuka* dan bisa diputar ceritanya.

### D. Cara Memutar Cerita (Bermain Dialog) Lewat UI
Ketika *player* membuka layar Koleksi (*Inventory/Summary Panel*), lalu mengeklik item shard-nya:
1. Hubungkan logika Event Tombol klik Anda agar memanggil kode:
   ```csharp
   memoryShardManager.PlayShardDialogue(referensiDataShard);
   ```
2. Otomatis, dialog visual novel akan mengambil alih layar, dan pemain bisa membaca rentetan cerita (selama *shard* sudah dibuka lewat Langkah C).

---

Selesai! Dengan arsitektur ini, seluruh sistem di game `Toba-The-Broken-Vow` baik itu Dialog per *Chapter*, maupun dialog cerita dari kepingan masa lalu (*Memory Shard*), sekarang berada dalam 1 sistem modular (*ScriptableObject*) yang konsisten dan lebih ringan dibandingkan *cutscene director* yang berlapis-lapis.
