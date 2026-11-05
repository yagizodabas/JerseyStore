# Katkı ve geliştirme

1. .NET 10 SDK ile `dotnet restore JerseyStore.slnx --locked-mode` çalıştırın.
2. `README.md` üzerinden ayrı bir geliştirme veritabanı kurun. Gerçek müşteri verisi kullanmayın.
3. Bir çalışma dalı açın; değişiklikleri küçük, açıklayıcı commit'lerle kaydedin.
4. `dotnet build JerseyStore.slnx --no-restore --configuration Release --warnaserror` çalıştırın.
5. Uygulama açıkken PowerShell 7 ile `./scripts/smoke.ps1 -BaseUrl http://localhost:5062` çalıştırın.
6. Arayüz değişikliklerini masaüstü ve dar ekranda; SQL değişikliklerini `docs/verification.md` senaryolarıyla kontrol edin.
7. PR açıklamasında davranış değişikliklerini, çalıştırılan kontrolleri ve kalan sınırları belirtin.

Bağımlılık değişikliğinde `dotnet restore JerseyStore.slnx --force-evaluate` çalıştırıp `JerseyStore/packages.lock.json` dosyasını da commit edin. Parolaları ortam değişkeninde veya .NET user-secrets içinde tutun. Şema dosyası mevcut veritabanına uygulanacak bir migration değildir.
