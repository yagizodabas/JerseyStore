# Depo incelemesi — 5 Kasım 2026

Bu rapor [PR #1](https://github.com/yagizodabas/JerseyStore/pull/1) kapsamındaki inceleme ve düzeltmeleri belgeler. Birleştirme ve otomatik kontrol durumu PR sayfasından izlenebilir.

## Dal ve dosya düzeni

- `master`: varsayılan dal; kullanıcıların klonladığında aldığı sürüm. Kök README ve uygulama belgeleri bu sürümle birlikte tutulur.
- `pys_23`: ürün ve sepet ekranlarını içeren mevcut çalışma dalıydı. Bu dalın geçmişi PR #1 dalına birleştirildi; kaynak dal silinmedi.
- `improve/project-quality`: PR #1 kapsamındaki bütünleşik sürüm. Ana daldaki README eklemesiyle oluşan çakışma giderildi.
- `JerseyStore.slnx` ve `README.md` kökte, uygulama kaynakları `JerseyStore/` altında doğru konumdadır. `src/` klasörü eklemek tek uygulamalı bu depo için zorunlu değildir.
- `OrdersControllers.cs`, içerdiği `OrdersController` sınıfına uygun adlandırıldı.
- Katalog tek Razor görünümünde toplandı; `/Home/Index` kataloğa yönlenir. Sepet `/Orders/Cart` altında korunur.
- Kullanılmayan şablon ana sayfası, eski layout stili ve boş `site.js` kaldırıldı. Üçüncü taraf kütüphanelerin lisans dosyaları korundu.

## Uygulanan düzeltmeler

- Negatif/sıfır/aşırı adet, boş sepet ve alan uzunlukları sunucuda doğrulanır.
- Stok koşullu atomik güncellemeyle azaltılır; başarısız siparişin stok değişiklikleri geri alınır.
- Ürün ve kişiselleştirme metinleri HTML olarak çalıştırılmadan gösterilir. Dinamik `innerHTML` ve satır içi `onclick` kaldırıldı.
- Tarayıcı sepeti okunurken biçim kontrolü yapılır; depolama hataları kullanıcıya gösterilir.
- Gönderim sırasında tekrar tıklama engellenir. Sonucu belirsiz gönderimde otomatik tekrar yapılmaz ve kullanıcı uyarılır. Bu, sunucu tarafında idempotency yerine geçmez.
- Şablon gizlilik ve hata sayfaları gerçek prototip davranışına uygun Türkçe metinle değiştirildi.
- Şema, kurulum, test adımları ve katkı belgesi eklendi. Belge bağlantıları ve sayfa/statik dosya yolları kontrol edildi.
- .NET 10 SDK seçimi ve NuGet kilit dosyası eklendi; CI kilitli restore kullanır.
- Yerel `.env`, `appsettings.Local.json` ve SQL yedek dosyaları Git dışında tutulur. `appsettings.Local.json` otomatik yüklenen bir yapılandırma değildir; ortam değişkeni/user-secrets tercih edilir.

## Kontrol sonuçları

- Release derlemesi: 0 uyarı, 0 hata.
- 11 geçersiz sipariş isteği: beklenen 400 yanıtları.
- Ayrı SQL Express veritabanında başarılı sipariş ve sunucu toplamı: geçti.
- Tekrarlanan ürün satırında yetersiz stok ve olmayan ürün nedeniyle rollback: geçti; stok ve sipariş satırları SQL ile doğrulandı.
- Son bir ürün için iki eşzamanlı sipariş: biri 200, diğeri 409; son stok 0.
- Tarayıcıda sepete ekleme ve sentetik bilgilerle sipariş gönderimi: geçti; başarılı yanıtta sepet temizlendi.
- HTML benzeri takım/baskı adlarının düz metin gösterimi: geçti.
- 390 piksel ekran genişliğinde katalog ve sepet incelendi. Sepet tablosu kendi yatay kaydırma alanındadır; sayfa genişliğinde taşma gözlenmedi.

Testler yalnızca bu inceleme için oluşturulan `JerseyStore_Verification_01a1abc4` veritabanında sentetik verilerle yapıldı. Mevcut mağaza veritabanına yazılmadı.

## Kalan işler ve sınırlar

1. Dal ve yayın durumu PR sayfasından izlenmelidir; GitHub ana dal güncellemesi canlı bir siteye dağıtım anlamına gelmez.
2. Mevcut veritabanı şeması paylaşılmadığından canlı veritabanına uyumluluk doğrulanmadı. Özellikle OrderNumber en az 34 karakter olmalıdır. Yeni kurulum şeması mevcut tablolar üzerinde çalıştırılmaz.
3. Ödeme, hesap/yetki, yönetim, kargo ve sipariş durumu özellikleri yoktur. Sunucu tarafı idempotency ve kötüye kullanım sınırlandırması üretim öncesi ele alınmalıdır.
4. Bir proje lisansı seçilmemiştir. Hak sahibinin kararı olmadan MIT/Apache gibi bir lisans eklenmedi.
5. Canlı site adresi ve gerçek ürün görselleri sağlanmadı. Sahte adres veya ürün görseli eklenmedi.
