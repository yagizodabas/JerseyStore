# İşlevsel doğrulama

Ayrı, geçici bir SQL Server test veritabanı kullanın; gerçek müşteri verisi kullanmayın. `database/schema.sql` ile tabloları oluşturup fiyatı 100, stoğu 3 olan bir ürün ekleyin. Uygulamanın bağlantısını bu test veritabanına yönlendirin. İstekleri geliştirme ortamındaki Swagger arayüzünden gönderebilirsiniz.

1. Ürün listesi doğru fiyat ve stoğu döndürmeli.
2. Adet 0, -1, 101; ürün kimliği 0; boş müşteri/adres/telefon; numara 100 ve boş sepet istekleri 400 döndürmeli. Veritabanı değişmemeli.
3. Adet 2 olan geçerli sipariş 200 döndürmeli. Toplam 200, kalan stok 1 olmalı; Orders ve OrderItems kayıtları eşleşmeli.
4. Aynı sepette aynı üründen iki ayrı satırla toplam 2 adet isteyin. Kalan stok 1 olduğundan 409 dönmeli; stok 1 kalmalı, yeni sipariş oluşmamalı.
5. Olmayan ürün ve geçerli ürünü aynı sepette gönderin. 409 dönmeli ve geçerli ürünün stoğu değişmemeli.
6. Stoğu 1 olan aynı ürün için iki eşzamanlı, birer adet sipariş gönderin. Yalnızca biri başarılı olmalı, diğeri 409 dönmeli; stok 0 olmalı.
7. SQL Server erişimini test bağlantısında kesin. Ürün ve geçerli sipariş istekleri 503 dönmeli; istemciye SQL ayrıntıları gönderilmemeli.

Bu kontroller gerçek SQL Server gerektirir; yalnızca derleme başarısı işlemsel veya eşzamanlı davranışı kanıtlamaz.

## Bu değişiklikte çalıştırılan kontroller

- .NET 10 Release derlemesi: 0 uyarı, 0 hata.
- PowerShell HTTP testi: 11 geçersiz sipariş ve katalog sayfası başarılı.
- Erişilemeyen veritabanıyla ürün listesi: beklenen 503 yanıtı başarılı.
- SQL Server üzerinde gerçek sipariş/rollback/eşzamanlılık testleri çalıştırılmadı.
- Tarayıcıda görsel doğrulama yapılmadı; katalog kontrolü HTML yanıtıyla sınırlıdır.
