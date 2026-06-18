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
 │    └── FadeOverlay (Cukup pakai komponen Image biasa, ubah Color jadi hitam. Tidak butuh aset gambar luar)
 ├── LeftPortraitRoot (Image) -> *Tambahkan skrip PortraitSlot.cs di sini*
 ├── CenterPortraitRoot (Image) -> *Tambahkan skrip PortraitSlot.cs di sini*
 ├── RightPortraitRoot (Image) -> *Tambahkan skrip PortraitSlot.cs di sini*
 └── DialoguePanel (GameObject Panel biasa)
      ├── SpeakerText (TextMeshProUGUI)
      └── DialogueText (TextMeshProUGUI)
```

**Konfigurasi Object UI (Cara Memasang Komponen di Inspector):**
- **PortraitSlot.cs:**
  - Klik *GameObject* bernama `LeftPortraitRoot` di jendela *Hierarchy*.
  - Di jendela *Inspector*, cari skrip `PortraitSlot`. Pada kolom **Portrait Image**, tarik dan lepaskan (*drag and drop*) *GameObject* `LeftPortraitRoot` itu sendiri ke dalam kolom tersebut (Sistem Unity akan otomatis mengambil komponen *Image* yang menempel pada objek tersebut).
  - Pada kolom **Slot Position**, ubah posisinya menjadi `Left`. 
  - *(Ulangi langkah yang sama persis untuk objek CenterPortraitRoot dan RightPortraitRoot)*.
- **BackgroundFader.cs:**
  - Klik *GameObject* bernama `Background` di jendela *Hierarchy*.
  - Pada skrip `BackgroundFader` di jendela *Inspector*, tarik dan lepaskan *GameObject* `Background` itu sendiri ke dalam kolom **Background Image**.
  - Pada kolom **Fade Overlay**, tarik dan lepaskan *GameObject* bernama `FadeOverlay` (objek anaknya) ke dalam kolom tersebut.

### Pengaturan Latar Belakang (Transparan vs Tertutup Penuh)
Berdasarkan sistem *overlay* ini, panel dialog memang secara alami akan **menimpa layar gameplay utama Anda**, layaknya game RPG modern. Anda memegang kendali penuh atas seberapa pekat UI ini menutupi *gameplay* melalui pengaturan komponen *Image* pada objek `Background`:

- **Mode Gelap Transparan (Seperti di Screenshot):** Pada *GameObject* `Background`, atur warnanya menjadi Hitam, lalu turunkan nilai *Alpha* (A) menjadi sekitar `150` (dari maksimal 255). Ini akan meredupkan *gameplay* di belakangnya sehingga pemain fokus ke teks, namun situasi dunia *game* tetap terlihat.
- **Mode Tembus Pandang (Transparan Penuh):** Turunkan nilai *Alpha* hingga `0`. *Gameplay* akan terlihat 100% terang benderang, dan hanya kotak papan dialog saja yang muncul di bawah layar.
- **Mode Tertutup Gambar (Pindah Lokasi):** Jika Anda menggunakan fitur `Background Override` pada Data Dialog, sistem akan otomatis menempelkan gambar latar baru yang tidak tembus pandang (100% menutupi layar) jika adegan tersebut membutuhkannya.

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

## 7. Setup Efek Intro Chapter (Siluet, Zoom & Fade Out)

Sistem `ChapterManager` telah dimodifikasi agar dapat menampilkan animasi judul *Chapter* dan siluet gambar yang di-zoom serta memudar (*fade out*) sebelum dialog dimulai.

## 7. Setup Efek Intro Chapter (Siluet, Zoom & Fade Out)

Sistem `ChapterManager` telah dimodifikasi agar dapat menampilkan animasi judul *Chapter* dan siluet gambar yang di-zoom serta memudar (*fade out*) sebelum dialog dimulai.

### A. Membangun Struktur "Wadah" UI di Canvas
Langkah ini bertujuan membuat bentuk fisiknya di layar (*GameObject*).
1. Tambahkan sebuah **Panel** baru di dalam Canvas (posisikan *layer/hierarchy*-nya di urutan paling bawah agar menutupi seluruh objek UI lain di layar). Beri nama `IntroPanel`.
2. Ubah warna komponen *Image* pada `IntroPanel` menjadi Hitam pekat (Alpha = 255).
3. Tambahkan komponen **CanvasGroup** ke `IntroPanel`. Komponen ini diwajibkan agar skrip nantinya bisa menciptakan efek layar transparan memudar (*Fade Out*).
4. Buat objek teks (TextMeshPro) di dalam `IntroPanel` untuk tulisan Chapter. Beri nama `ChapterText` dan posisikan di tengah atas layar.
5. Buat objek **Image** di dalam `IntroPanel` untuk menaruh bayangan siluet. Beri nama `SilhouetteImage`. Posisikan ukurannya agar cukup besar di layar.
6. Buat objek teks (TextMeshPro) baru di dalam `IntroPanel` khusus untuk tulisan dongeng/narator. Beri nama `LoreText` dan posisikan di bawah letak siluet.

### B. Konfigurasi "Intro Sequence UI" (Menyambungkan Wadah)
**Intro Sequence UI** adalah tempat Anda memberi tahu skrip, *"Objek mana saja di layar yang harus saya kendalikan/ubah-ubah bentuknya?"*. Jika ini kosong, skrip akan kebingungan mencari teks atau gambar mana yang harus dimunculkan.
1. Klik *GameObject* `ChapterManager` di *Hierarchy*.
2. Pada *Inspector*, perhatikan kategori **Intro Sequence UI** dan **Lore Text Settings (Opsional)**.
3. Tarik dan hubungkan (*Drag and Drop*) objek riil yang baru Anda buat dari jendela *Hierarchy* ke kolom yang sesuai:
   - **Intro Panel:** Tarik objek `IntroPanel`.
   - **Intro Canvas Group:** Tarik objek `IntroPanel` (Unity otomatis mengambil komponen CanvasGroup-nya).
   - **Chapter Name Text:** Tarik objek `ChapterText`.
   - **Silhouette Image:** Tarik objek `SilhouetteImage`.
   - **Lore Text UI:** Tarik objek `LoreText`.

### C. Konfigurasi "Intro Settings" (Mengatur Isi Data & Durasi Waktu)
Berbeda dengan UI di atas yang berisikan *GameObject*, **Intro Settings** adalah tempat Anda mengatur nilai kontennya (*Value*), seperti teks spesifik apa yang mau ditulis, gambar aset 2D (*Sprite*) mana yang mau dipajang, dan kecepatan waktu pergerakannya.
1. Masih di *Inspector* skrip `ChapterManager`, cari kategori **Intro Settings** dan atur konten aslinya:
   - **Chapter Name:** Ketik "Chapter 1" atau judul bab yang sedang berjalan.
   - **Silhouette Sprite:** Masukkan aset gambar/ikon 2D siluet pemancing.
   - **Lore Text Content:** Ketik tulisan ceritanya di sini (Misal: *"Dahulu, di sebuah lembah kering yang subur..."*).
2. Atur ketukan waktu animasi (dalam hitungan detik):
   - **Wait Before Silhouette:** Jeda waktu layar dibiarkan kosong dengan teks Chapter saja sebelum siluet tiba-tiba muncul (Misal `1.5` detik).
   - **Wait Before Lore:** Jeda waktu setelah siluet muncul sebelum kalimat cerita mulai diketik mesin tik (Misal `0.5` detik).
   - **Lore Reading Duration:** Jeda waktu tunggu layar dibekukan agar pemain punya kesempatan menyelesaikan bacaannya sebelum animasi dilanjutkan (Misal `4.0` detik).
   - **Zoom And Fade Duration:** Durasi proses layar hitam menghilang (*Fade Out*) secara berbarengan dengan efek *Zoom In* siluet ke arah dunia *Gameplay* (Misal `2.0` detik).

**Urutan Alur Kejadian Saat Dimainkan:** 
Layar Hitam (Teks Chapter muncul) -> *(Jeda 1.5 detik)* -> Gambar Siluet muncul -> *(Jeda 0.5 detik)* -> Teks Lore diketik -> *(Jeda 4 detik membaca)* -> Layar memudar perlahan sembari siluet membesar mendekat layar -> Otomatis masuk ke percakapan Visual Novel utama!

---

## 8. Integrasi Puzzle Patung dengan Memory Shard (Trigger Cutscene)

Untuk merajut dunia eksplorasi *gameplay* utama dengan sistem Visual Novel, kita telah merancang agar *cutscene* (kilasan memori) hanya bisa diputar sebagai *reward* (hadiah) ketika pemain berhasil memecahkan teka-teki patung. 

### A. Membangun UI Popup "Memory Shard"
Langkah pertama adalah membuat antarmuka pemberitahuan bahwa pemain mendapat memori baru.
1. Di dalam `Canvas` UI Utama (tempat berkumpulnya UI *Gameplay*), buat objek **Panel** baru dan beri nama `MemoryShardPopup`. Atur ukurannya agar estetik di tengah layar.
2. Tambahkan **TextMeshPro** ke dalamnya bertuliskan peringatan, misal: *"Kepingan Memori Masa Lalu Ditemukan!"*.
3. Buat dua buah **Button** di dalam panel tersebut:
   - Tombol 1: Beri nama `TontonButton` (Isi teksnya: *"Tonton Sekarang"*).
   - Tombol 2: Beri nama `TutupButton` (Isi teksnya: *"Simpan untuk Nanti"*).
4. Setelah desainnya selesai, **matikan (Uncheck)** objek `MemoryShardPopup` di *Inspector* agar tidak menutupi *gameplay* saat baru mulai bermain.

### B. Konfigurasi MemoryShardManager
Ini adalah skrip sentral yang bertugas memunculkan UI Popup saat patung berhasil dipecahkan.
1. Buat *GameObject* kosong baru di jendela *Hierarchy*, beri nama `MemoryShardManager`.
2. Tarik dan pasangkan skrip `MemoryShardManager.cs` ke objek tersebut.
3. Di komponen *Inspector*, isikan kolom referensinya dengan menggeser objek dari *Hierarchy*:
   - **Target Chapter Manager:** Tarik *GameObject* `ChapterManager` ke sini (agar sistem tahu *Cutscene* mana yang kelak diputar).
   - **Popup Panel:** Tarik objek `MemoryShardPopup` yang baru Anda buat di langkah A.
   - **Tonton Button:** Tarik objek `TontonButton`.
   - **Tutup Button:** Tarik objek `TutupButton`.
*(Catatan: Anda tidak perlu men-setting event OnClick() pada tombol di Unity Inspector, karena skrip akan mengikat fungsi tombolnya secara otomatis!).*

### C. Konfigurasi Patung (PatungStatue.cs)
Bagian ini mengatur agar patung mengirimkan sinyal hadiah memori saat alat pancing dipasang.
1. Klik objek patung yang memiliki skrip `PatungStatue` di *Hierarchy*.
2. Pada *Inspector* skrip tersebut, akan ada kolom rumpang baru:
   - **Memory Shard ID:** Biarkan nilainya `Chapter1` (ini sebagai kunci identitas).
3. **PENTING:** Pastikan Anda sudah mematikan (Uncheck) opsi **Play On Start** pada komponen `ChapterManager`, agar *cutscene* tidak bocor dan terputar otomatis di awal game.

### D. Urutan Alur Kejadian Nyata (Gameplay Flow):
1. Pemain membawa alat pancing (*Fishing Rod*).
2. Pemain menekan tombol interaksi di depan Patung.
3. Skrip `PatungStatue` bereaksi: meletakkan kail ke patung, mematikan efek kedip, dan membuka pintu rahasia.
4. Di detik yang sama, patung mengirim sinyal ke sistem: *"Munculkan hadiah Memory Shard!"*
5. Skrip `MemoryShardManager` menanggapi dengan memunculkan layar `MemoryShardPopup` menutupi layar pemain.
6. Pemain mengklik tombol **"Tonton Sekarang"**.
7. `MemoryShardManager` langsung menutup UI popup tersebut, dan meneruskan perintah ke `ChapterManager`.
8. Efek layar menjadi Hitam, Siluet muncul, dan transisi dramatis Visual Novel *Chapter 1* pun dimulai!

---

## 9. Konsep 3 Manager (Jika Ingin Membuat Chapter/Memori Baru)

Sistem penceritaan game ini dibangun di atas 3 pilar Manager utama yang saling mengoperkan tugas. Berikut adalah penjelasan peran mereka masing-masing agar Anda tidak bingung:

1. **MemoryShardManager (Sang Penjaga Pintu Gameplay)**
   - **Peran:** Dia yang hidup di dunia 3D/2D Anda. Tugasnya mendeteksi apakah pemain sudah memecahkan teka-teki, mengambil barang, atau menyentuh *shard*. 
   - **Output:** Dia yang memunculkan UI *Popup* "Tonton Sekarang", menyimpan data koleksi, lalu mengoper kendali ke *ChapterManager*.
2. **ChapterManager (Sang Sutradara Intro)**
   - **Peran:** Dia yang mengatur suasana sinematik *sebelum* orang-orang mulai mengobrol.
   - **Output:** Menghitamkan layar, memunculkan siluet raksasa, mengetik teks cerita dongeng (Lore Text), membesarkan (*zoom*) layar, dan melempar jalannya cerita ke *DialogueManager*.
3. **DialogueManagerCS (Mesin Visual Novel)**
   - **Peran:** Dia adalah pemutar kaset percakapannya.
   - **Output:** Membaca data naskah, memunculkan wajah (*portrait*) karakter Samosir/Toba di layar, meredupkan wajah karakter yang sedang diam, mengetik obrolan kata per kata, hingga adegannya selesai.

### "Lalu, jika saya ingin membuat Memory Shard / Chapter 2 yang baru, saya harus buat apa saja?"
Setiap kali Anda ingin menambah *Cutscene* / Ingatan baru (misal: Chapter 2), ikuti *checklist* sederhana ini:

1. **Buat Naskahnya (`VNDialogueData`):**
   - Klik kanan di folder *Project* -> *Create > Visual Novel > Dialogue Data*. Beri nama `Chapter2_Data`.
   - Isi baris percakapan antara karakter di dalamnya.
2. **Siapkan Sutradaranya (`ChapterManager`):**
   - Buat *GameObject* kosong di *Hierarchy*, beri nama `ChapterManager_2`.
   - Masukkan komponen `ChapterManager`.
   - Tarik naskah `Chapter2_Data` ke kolom *Chapter Intro Data*.
   - Ubah teks Lore dan Siluet khusus untuk Chapter 2 di bagian *Intro Settings*.
   - *(Ingat: Tetap matikan opsi Play On Start!)*
3. **Panggil dari Dunia Game (Pemicu / Trigger):**
   - Pasang skrip rahasia di sebuah *Item* atau *Patung* yang ada di dalam map Anda.
   - Di dalam skrip *Item* tersebut, perintahkan untuk memanggil: `ChapterManager_2.TriggerChapterIntro();` (Atau jika memanggil lewat UI Popup, masukkan `ChapterManager_2` ini ke dalam kolom target milik `MemoryShardManager`).

Dengan pola 3 langkah ini, Anda bisa memproduksi puluhan ingatan masa lalu *(Memory Shards)* dan Chapter yang berbeda-beda tanpa perlu membuat skrip kode C# yang baru sama sekali!
