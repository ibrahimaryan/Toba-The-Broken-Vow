# Panduan Implementasi Prologue (Best Practice)
Project: Toba - The Broken Vow

Dokumen ini berisi panduan langkah demi langkah tentang cara mengatur dan menampilkan Prologue di awal permainan, memanfaatkan fitur `isPrologueCenterText` yang sudah ditambahkan ke dalam `DialogueManagerCS`.

## Konsep Dasar
Alih-alih membuat Script/Manager baru yang rumit khusus untuk Prologue, kita menggunakan **DialogueManagerCS** yang sudah ada. Keuntungannya:
- Hemat memori dan performa (tidak ada script tumpang tindih).
- Fitur SFX (seperti suara petir atau ketikan) langsung bisa dipakai.
- Sistem transisi teks (klik-untuk-lanjut) sudah teruji dan tertangani otomatis.

---

## Tahap 1: Persiapan UI di Canvas

1. Buka Scene awal Anda di Unity.
2. Di dalam jendela **Hierarchy**, cari Canvas utama Anda.
3. Buat sebuah **UI -> Panel** baru. 
   - Ubah namanya menjadi `ProloguePanel`.
   - Ubah warnanya menjadi **Hitam Pekat (Black)** dengan transparansi (Alpha) maksimal (255).
4. Klik kanan pada `ProloguePanel` yang baru dibuat, lalu buat **UI -> Text - TextMeshPro**.
   - Ubah namanya menjadi `PrologueText`.
   - Atur *font*, ukuran, dan gayanya agar mudah dibaca.
   - Atur **Alignment** menjadi rata tengah (*Center* & *Middle*).
5. Cari objek `DialogueManager` di Hierarchy.
6. Di dalam *Inspector* komponen `DialogueManagerCS`, Anda akan melihat pengaturan baru di bawah header **Prologue UI (Center Text)**:
   - Tarik (Drag) `ProloguePanel` ke slot **Prologue Panel**.
   - Tarik (Drag) `PrologueText` ke slot **Prologue Text**.
7. *(Opsional)* Matikan tanda centang di sebelah nama `ProloguePanel` untuk menyembunyikannya sementara saat Anda mengedit map. Sistem akan otomatis menyalakannya kembali saat game dimainkan.

---

## Tahap 2: Pembuatan Data Dialog Prologue

1. Di jendela **Project**, buka folder `Assets/Assets/DialogVisualNovel/Characters/Dialogues`.
2. Klik Kanan -> **Create -> VN -> Dialogue Data**.
3. Beri nama file baru tersebut: `Prolog_Data`.
4. Buka file tersebut di Inspector dan tambahkan elemen (baris) dialog seperti biasa. Tulis teks pengantar cerita Anda.
5. **[SANGAT PENTING]** Pada setiap baris di `Prolog_Data` ini, **CENTANG kotak `[x] Is Prologue Center Text`**.
6. *(Opsional)* Jika Anda ingin ada efek suara saat kalimat tertentu muncul, masukkan file *AudioClip*-nya ke slot **Sfx Clip** di baris tersebut.

---

## Tahap 3: Pemicu (Trigger) Menjalankan Prologue

Anda hanya perlu memanggil fungsi `PlayDialogue` persis seperti saat Anda menjalankan dialog Visual Novel biasa. Ada dua skenario paling umum:

### Skenario 1: Digabungkan dengan ChapterManager
Jika Anda ingin prolog muncul tepat sebelum judul Chapter 1 memudar:
1. Klik objek `ChapterManager`.
2. Masukkan file `Prolog_Data` Anda ke dalam slot **Chapter Intro Data**.
3. *Selesai!* Saat game dimulai, kode terbaru kita akan otomatis menjalankan prolog ini dengan layar hitam. Setelah dialog prolog di-klik sampai habis, siluet akan memudar dengan mulus (*Seamless Crossfade*) memperlihatkan *gameplay*.

### Skenario 2: Dipicu dari Tombol "New Game" (Main Menu)
Jika Prologue Anda berada di Scene terpisah setelah Main Menu:
1. Buat skrip pemicu sederhana di awal scene yang memanggil `dialogueManager.PlayDialogue(prolog_Data);`
2. Setelah dialog selesai, Anda bisa memuat (load) Scene Chapter 1.

---

## Kesimpulan
Dengan arsitektur ini, sistem Visual Novel Anda menjadi sangat *modular* dan efisien. Anda bisa menggunakan `DialogueManagerCS` untuk percakapan karakter biasa, namun bisa seketika menyulapnya menjadi teks pengantar narator (Prologue) di tengah layar hanya dengan mengaktifkan satu Checkbox di file Data Dialog.
