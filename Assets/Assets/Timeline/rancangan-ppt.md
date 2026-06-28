# Rancangan Presentasi (PPT) Game: Toba The Broken Vow

*Dokumen ini berisi draf atau rancangan pembagian materi presentasi untuk setiap anggota tim.*

---

## 1. Judul (Slide Pembuka)
- **Judul Game:** Toba: The Broken Vow
- **Genre:** 2D Narrative-Puzzle Adventure
- **Tim Pengembang:** Ahmad Rizkiadi Budi Wirawan, Fajar Wahyu Suryaputra, Ibrahim Aryan Faridzi, Miftachussurur

---

## 2. Bagian: Surur

### Elemen Game
- **Genre & Style:** Narrative-Puzzle dipadukan dengan Visual Novel (VN).
- **Mekanik Inti:** Eksplorasi lingkungan, pemecahan teka-teki (*casual puzzle*), dan pengumpulan *Memory Shards*.
- **Fokus Utama:** *Storytelling through Reconstruction* (menyampaikan cerita sebagai bentuk *reward* setelah teka-teki selesai).

### Player
- **Jumlah Pemain:** *Single-player* (Satu pemain).

### Target Pemain
- **Usia:** Remaja hingga Dewasa (13+).
- **Demografi/Minat:** Penggemar game berbasis cerita (*story-rich*), penyuka teka-teki santai, dan audiens yang tertarik dengan *folklore* atau budaya lokal Nusantara.

### Technical Form
- **Engine:** Unity (Bahasa pemrograman C#).
- **Dimensi & Grafis:** 2D (Eksplorasi) dipadukan dengan UI Kanvas statis (Visual Novel).
- **Manajemen Data:** Mengandalkan *ScriptableObject* untuk performa memori yang efisien (menyimpan teks dialog dan potret karakter).

### Camera
- **Jenis Kamera:** 2D Orthographic Camera.
- **Perilaku:** 
  - *Follow Camera*: Mengikuti pergerakan pemain saat fase eksplorasi.
  - *Static Camera*: Mengunci di tengah layar ketika pemain memasuki adegan *cutscene* atau dialog Visual Novel.

### Platform
- **Target Utama:** PC (Desktop/Laptop).
- **Format Rilis:** *Executable File* (.exe) untuk sistem operasi Windows.

### Language
- **Bahasa Utama:** Bahasa Indonesia (dirancang dengan tata bahasa naratif yang emosional).

### Device
- **Perangkat Keras:** PC / Laptop (dengan spesifikasi ringan menengah).

---

## 3. Bagian: Him

### Kontrol
- **Gerakan (Movement):** Tombol WASD atau Arrow Keys.
- **Interaksi (Action):** 
  - Menekan tombol **'E'** atau **Klik Kiri Mouse** untuk berinteraksi dengan objek lingkungan, memungut item, dan melanjutkan dialog bacaan.
  - Tombol **'I'** (atau *on-screen button*) untuk membuka menu Misi (To-Do List) dan *Memory Shards*.
- **Navigasi UI:** Menggunakan *Mouse/Cursor*.

### Game Level (Gameplay)
- Permainan dibagi menjadi beberapa lokasi penting yang merepresentasikan tahapan emosional:
  1. **Level 1 (Gubuk Toba):** Masa Kini/Awal Petualangan, berfokus pada pengenalan harmoni masa lalu keluarga.
  2. **Level 2 (Tepi Sungai Tua):** Mengenang awal pertemuan mistis dan ikrar suci antara Toba dan Putri.
  3. **Level 3 (Ladang Pertanian):** Puncak konflik domestik, di mana kelelahan bekerja dan kelaparan memicu letupan amarah.
  4. **Level 4 (Rumah Opung):** Eksplorasi untuk merangkai petunjuk spiritual dan wejangan tetua.
  5. **Level 5 (Puncak Bukit):** Area klimaks tragedi (*The Broken Vow*) di mana desa akhirnya tenggelam menjadi danau.

---

## 4. Bagian: Fajar

### Gameplay
- **Opening:** 
  - Pemain memulai permainan di dunia pasca-tragedi (setelah desa tenggelam). Narasi disajikan secara terfragmentasi, memacu rasa penasaran pemain untuk mencari tahu *bagaimana* dan *mengapa* hal ini terjadi melalui sudut pandang *flashback*.
- **Sinopsis:**
  - Mengangkat kembali Legenda Danau Toba. Toba, seorang petani tangguh, melanggar janji sakral kepada istrinya (Putri, jelmaan ikan emas) akibat emosi memuncak terhadap putranya, Samosir. Game ini membongkar psikologis dan penyesalan di balik "sumpah yang teringkari" tersebut yang mendatangkan musibah air bah.
- **Mode:**
  - *Story/Campaign Mode* (Satu alur cerita linier yang disusun melalui teka-teki).

---

## 5. Bagian: Riski

### Key Fitur
- **Exploration:** Menjelajahi berbagai lingkungan yang kaya akan rahasia tersembunyi dan petunjuk cerita.
- **Narrative Adventure:** Petualangan yang digerakkan oleh alur cerita yang kuat (Legenda Danau Toba).
- **Memory Shard:** Mekanik utama di mana cerita masa lalu diungkap melalui kepingan memori yang berhasil disusun pemain.
- **Collectible Item:** Berbagai artefak dan objek yang dapat ditemukan selama eksplorasi untuk memecahkan puzzle atau memperdalam *lore* permainan.

### Karakter
1. **Toba:** Protagonis (petani); pekerja keras namun emosinya labil saat kelelahan fisik.
2. **Putri:** Jelmaan ikan bersisik emas; anggun, tenang, penjaga rahasia sekaligus penahan emosi keluarga.
3. **Samosir:** Anak dari Toba dan Putri; polos, sangat aktif, selalu lapar, dan keteledorannya memicu konflik utama.
4. **Opung:** Sesepuh/tetua desa; bertindak secara naratif sebagai pemberi wejangan dan petunjuk teka-teki (*hints*) kepada pemain.
