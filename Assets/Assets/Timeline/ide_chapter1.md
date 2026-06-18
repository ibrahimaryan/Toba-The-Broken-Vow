# Panduan Pembuatan: Cutscene 1 (Chapter 1)

Berikut adalah panduan praktis dan cara *"mengakali"* sistem Visual Novel yang telah kita buat untuk mengeksekusi naskah dialog Chapter 1 secara dramatis tanpa perlu menulis kode baru.

## Naskah Referensi dari Anda
> **(Layar transisi kilasan memori, menampilkan siluet Toba di tepi sungai kuno)**
> **Narator/Lore:** "Dahulu, di sebuah lembah kering yang subur, hiduplah seorang pemuda yatim piatu bernama Toba. Ia menyambung hidup dengan bertani dan memancing."
> **Samosir (gambar mengecil):** "Ayahh??"
> **Toba (Memori):** "Sudah seharian penuh aku melempar kail, tak satu pun ikan yang mendekat... Apakah hari ini aku harus kelaparan?"
> *(Siluet joran melengkung tajam, cahaya emas berpendar)*
> **Toba (Memori):** "Tunggu... tarikan ini kuat sekali! Ikan apa ini? Sisiknya... berkilau seperti emas murni! Aku belum pernah melihat ikan seajaib ini di sungai mana pun!"
> *(Fade hitam memudar ke scene chapter)*
> **Samosir (Kembali ke masa kini):** "Pria pemancing tadi... namanya Toba? Ayahkuu?? Kenapa ingatan tentang dirinya terasa begitu nyata di kepalaku?"

---

## 1. Persiapan Aset (Data Karakter & Gambar)
Sebelum meracik dialog, siapkan aset-aset berikut di *Project*:
- **Character Data `Char_Samosir`**: Siapkan *sprite* Samosir. Untuk membuat adegan "gambar mengecil", Anda cukup membuat file gambar (.png) Samosir dengan resolusi/skala yang lebih kecil lalu masukkan gambar tersebut ke dalam slot ekspresi **Confused** di data karakter ini.
- **Character Data `Char_Toba_Memori`**: Siapkan gambar/siluet Toba. Masukkan gambar Toba berekspresi kaget/terkejut ke dalam slot ekspresi **Shocked**.
- **Gambar Background**: Siapkan aset gambar `Bg_KilasanMemori` (Latar hitam dengan tepian blur/efek memori) dan gambar `Bg_CahayaEmas` (Layar yang dipenuhi sinar emas terang).

---

## 2. Pengaturan Layar Hitam & Teks Lore (Di ChapterManager)
Langkah ini untuk mengeksekusi bagian awal naskah sebelum percakapan antar karakter dimulai.

**Menambahkan Background Hitam Gradasi untuk Teks Lore**
Agar teks lore lebih mudah dibaca dan tidak bertabrakan dengan gambar siluet, kita akan memberikan latar hitam gradasi (menutupi sekitar 25% layar bawah):
1. Buka *GameObject* **IntroPanel** di *Hierarchy*.
2. Klik kanan pada **IntroPanel** -> *UI* -> *Image*. Beri nama **LoreBackground**.
3. Geser posisi **LoreBackground** di dalam *Hierarchy* agar berada tepat **di atas** `LoreText` (supaya ia digambar di belakang teks, bukan menimpa teks).
4. Tarik dan atur ukuran *Rect Transform* **LoreBackground** hingga menutupi area bawah layar (sekitar 25%-30% ketinggian layar tempat teks muncul).
5. Pada komponen *Image*, ubah *Color* menjadi Hitam.
6. **(Kunci Estetika):** Pada kolom *Source Image*, masukkan gambar/sprite *Gradient Hitam ke Transparan* (Anda bisa membuatnya di Photoshop atau mencari aset "Gradient Fade Sprite"). Ini akan membuat batas tepian background menyatu dengan mulus ke gambar siluet di atasnya.

Pilih *GameObject* **ChapterManager** di jendela *Hierarchy*, lalu atur propertinya:
- **Chapter Name:** `Chapter 1` (Boleh dikosongkan jika Anda tidak ingin tulisan ini ada).
- **Silhouette Sprite:** Masukkan gambar siluet pemandangan Toba memancing di tepi sungai kuno.
- **Lore Text Content:** Ketikkan: *"Dahulu, di sebuah lembah kering yang subur, hiduplah seorang pemuda yatim piatu bernama Toba. Ia menyambung hidup dengan bertani dan memancing."*
- **Zoom And Fade Duration:** Atur cukup lambat (misal `2.5` detik) agar transisi layar hitam menuju percakapan terasa berkesan.

---

## 3. Meracik Baris Dialog (Di VNDialogueData)
Buat file data baru bernama `Chapter1_Data` (*Create > Visual Novel > Dialogue Data*). Lalu tambahkan 5 elemen *Lines* dan atur persis seperti ini:

### Line 0 (Samosir Bingung)
- **Speaker:** `Char_Samosir`
- **Text:** "Ayahh??"
- **Emotion:** `Confused` *(Akan memanggil gambar Samosir yang "mengecil" karena sudah kita set di langkah 1)*.
- **Position:** `Left`
- **Background Override:** Masukkan gambar `Bg_KilasanMemori` agar layar gameplay tertutup dan memberi kesan sedang berhalusinasi/melihat masa lalu.

### Line 1 (Toba Mengeluh)
- **Speaker:** `Char_Toba_Memori`
- **Text:** "Sudah seharian penuh aku melempar kail, tak satu pun ikan yang mendekat... Apakah hari ini aku harus kelaparan?"
- **Emotion:** `Neutral`
- **Position:** `Right`
- **Background Override:** (Biarkan Kosong, ia akan mempertahankan gambar Bg_KilasanMemori dari Line 0).

### Line 2 (Kejadian Cahaya Emas)
Karena di teks ada instruksi *(cahaya emas berpendar)*, kita gunakan fitur *Background* untuk menyimulasikannya.
- **Speaker:** Kosongkan *(Agar kotak nama pembicara hilang).*
- **Text:** "Tiba-tiba, joran pancingnya melengkung tajam, dan seberkas cahaya emas menyilaukan memancar dari dalam air..."
- **Emotion:** `Neutral`
- **Position:** `Center`
- **Background Override:** Masukkan gambar `Bg_CahayaEmas`. *(Layar seketika akan menampilkan efek silau emas lewat fitur transisi Fade).*

### Line 3 (Toba Kaget)
- **Speaker:** `Char_Toba_Memori`
- **Text:** "Tunggu... tarikan ini kuat sekali! Ikan apa ini? Sisiknya... berkilau seperti emas murni! Aku belum pernah melihat ikan seajaib ini di sungai mana pun!"
- **Emotion:** `Shocked` *(Memanggil wajah Toba yang kaget)*.
- **Position:** `Right`
- **Background Override:** (Biarkan kosong).

### Line 4 (Kembali ke Masa Kini / Gameplay)
Untuk instruksi *(Fade hitam memudar ke scene chapter)*, kita "melepas" backgroundnya sehingga tembus pandang kembali ke *gameplay*.
- **Speaker:** `Char_Samosir`
- **Text:** "Pria pemancing tadi... namanya Toba?\nAyahkuu??\nKenapa ingatan tentang dirinya terasa begitu nyata di kepalaku?"
- **Emotion:** `Neutral`
- **Position:** `Left`
- **Background Override:** Klik logo bundar kecil di samping kolom ini, dan ubah menjadi **None** (kosong tanpa gambar). Ini akan memaksa layar transisi menghapus gambar silau emas tadi menjadi tembus pandang 100% (*kembali ke scene/dunia nyata*).

---

Dengan urutan *setup* tersebut, Anda bisa menerjemahkan hampir seluruh instruksi sutradara ke dalam program Visual Novel yang sudah jadi ini!
