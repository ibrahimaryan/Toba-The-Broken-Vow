# Panduan Pembuatan: Cutscene 2 (Memory Shard Chapter 2)

Panduan ini dibuat khusus untuk meracik adegan **Memory Shard Chapter 2**. Panduan ini juga membahas solusi dari beberapa masalah "error" atau panel hilang yang sempat Anda alami sebelumnya.

---

## 1. Persiapan Aset di Folder Project
Pastikan Anda sudah menyiapkan aset-aset berikut di folder `Assets/Assets/DialogVisualNovel/MemoryShards/Memory2/`:
- **Gambar Background**: Gambar yang ingin Anda tampilkan saat dialog berlangsung (misalnya `background-dialog.png` atau `Desain tanpa judul.png`).
- **Data Memory Shard (`MemoryData2.asset`)**: File Scriptable Object yang menyimpan data memori.
- **Data Dialog VN (`Chapter2_Intro.asset`)**: File Scriptable Object yang akan memutar urutan dialog.

---

## 2. Kenapa Sempat "Error" Terus?
Sebelum masuk ke pembuatan, mari kita pahami kenapa fitur ini sempat terasa membingungkan atau "error":
1. **Lupa Menghapus Centang "Is Unlocked"**: 
   Jika Anda sedang melakukan *testing* game, Anda wajib memastikan kotak `Is Unlocked` di file `MemoryData2.asset` dalam keadaan **kosong/False** sebelum menekan tombol Play. Jika ini tercentang, Unity mengira pemain sudah tamat, dan menolak memunculkan tombol "Tonton".
2. **Teks Dialog Kosong (Transparan)**:
   Karena Anda membuat visual novel *tanpa teks* (hanya mengandalkan gambar background), awalnya sistem menyembunyikan panel tersebut karena dianggap kosong. *(Namun tenang saja, sistem ini **sudah saya perbaiki**. Sekarang Anda bebas mengosongkan teks, dan gambarnya akan tetap muncul dengan sempurna!)*
3. **Konflik DontDestroyOnLoad**:
   Ini penyebab utama animasi layar hitam gagal muncul tadi. Namun berkat sistem **Auto-Recovery** yang sudah ditambahkan, Anda tidak perlu lagi khawatir soal urusan teknis ini. Sistem akan membereskan dirinya sendiri.

---

## 3. Pengaturan Layar Hitam, Nama Chapter, dan Teks Lore
Buka **ChapterManager** yang ada di *Hierarchy* (di dalam scene `chapter2_ruang_tamu`). Anda bisa mengatur sutradara pembukanya di Inspector:

- **Chapter Name:** Bebas diisi (misal: `"Chapter 2: Ruang Tamu"`).
- **Silhouette Sprite:** Masukkan gambar siluet/bayangan yang Anda inginkan.
- **Lore Text Content:** Ketik cerita pengantarnya (misal: *"Ingatan yang pudar mulai kembali merasuki pikiran..."*). Jika Anda tidak ingin ada teks cerita (ingin langsung siluet), **Hapus/kosongkan tulisan di kolom ini**.
- **Zoom And Fade Duration:** Atur ke `2.0` atau `3.0` detik untuk efek transisi yang dramatis.

---

## 4. Meracik Baris Dialog (Di `Chapter2_Intro.asset`)
Klik file `Chapter2_Intro.asset` Anda. Mari kita atur agar *Visual Novel* hanya menampilkan Background (sesuai kreativitas Anda sebelumnya):

### Line 0 (Memunculkan Background Saja)
- **Speaker:** *(Kosongkan)*
- **Text:** *(Kosongkan)*
- **Emotion:** `Neutral`
- **Position:** `Center`
- **Background Override:** Masukkan gambar `background-dialog.png` Anda ke sini! *(Inilah kunci utamanya! Mengisi Background Override akan menimpa seluruh layar gameplay dengan gambar tersebut).*

### Line 1 (Kembali ke Game)
Buat baris baru (+). Jika Anda ingin Cutscene ini langsung selesai dan pemain kembali bermain:
- **Speaker:** *(Bebas)*
- **Text:** *(Kosongkan, atau isi dengan titik "." jika butuh jeda)*
- **Background Override:** Klik logo bundar kecil di ujung kanan kolom ini, lalu pilih **None** (Silang). Ini adalah instruksi wajib bagi sistem untuk *"Menghapus gambar background tadi, dan jadikan layar kembali tembus pandang melihat karakter utama berjalan"*.

---

## 5. Pengujian Akhir
1. Klik file `MemoryData2.asset`. Pastikan **Is Unlocked** tidak dicentang.
2. Play game.
3. Selesaikan puzzle (dapatkan kail pancing / barang trigger).
4. Klik **"Tonton"**.
5. Nikmati hasilnya! Urutannya akan berjalan mulus: *Layar Hitam -> Nama Chapter -> Siluet -> Teks Cerita -> Zoom Out -> Background Dialog Anda*.
