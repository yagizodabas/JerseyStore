(() => {
    const grid = document.querySelector('#products');
    const status = document.querySelector('#catalog-status');
    const reload = document.querySelector('#reload-products');
    const cart = window.JerseyCart;
    function field(labelText, type, settings) {
        const label = document.createElement('label');
        label.textContent = labelText;
        const input = document.createElement('input');
        input.type = type;
        input.className = 'form-control';
        Object.assign(input, settings);
        label.append(input);
        return { label, input };
    }
    async function load() {
        reload.disabled = true;
        grid.setAttribute('aria-busy', 'true');
        grid.replaceChildren();
        status.textContent = 'Ürünler yükleniyor…';
        try {
            const response = await fetch('/api/store/products', { signal: AbortSignal.timeout(15000) });
            if (!response.ok) throw new Error('Catalog unavailable');
            const products = await response.json();
            if (!Array.isArray(products)) throw new Error('Invalid catalog');
            for (const product of products) {
                const card = document.createElement('article');
                card.className = 'product-card';
                const emblem = document.createElement('div');
                emblem.className = 'product-emblem';
                emblem.textContent = (product.teamName || '?').slice(0, 2).toLocaleUpperCase('tr-TR');
                emblem.setAttribute('aria-hidden', 'true');
                const title = document.createElement('h3');
                title.textContent = product.teamName;
                const amount = document.createElement('p');
                amount.className = 'product-price';
                amount.textContent = cart.money(product.price);
                const stock = document.createElement('p');
                stock.className = 'product-stock';
                stock.textContent = product.stock > 0 ? `${product.stock} adet stokta` : 'Stok tükendi';
                const form = document.createElement('form');
                form.className = 'product-options';
                const qty = field('Adet', 'number', { min: '1', max: String(Math.max(1, Math.min(100, product.stock))), value: '1', required: true });
                const number = field('Forma numarası (isteğe bağlı)', 'number', { min: '0', max: '99' });
                const name = field('Baskı adı (isteğe bağlı)', 'text', { maxLength: 40 });
                const add = document.createElement('button');
                add.type = 'submit';
                add.className = 'btn btn-dark';
                add.disabled = product.stock < 1;
                add.textContent = 'Sepete ekle';
                form.append(qty.label, number.label, name.label, add);
                form.addEventListener('submit', event => {
                    event.preventDefault();
                    if (!form.reportValidity()) return;
                    try {
                        const items = cart.read();
                        if (items.length >= 100) throw new Error('Sepete en fazla 100 satır ekleyebilirsiniz.');
                        const quantity = Number(qty.input.value);
                        const inCart = items.filter(item => item.productId === product.id).reduce((sum, item) => sum + item.quantity, 0);
                        if (inCart + quantity > product.stock) throw new Error('Sepetteki toplam adet mevcut stoğu aşıyor.');
                        items.push({ productId: product.id, teamName: product.teamName, price: product.price, quantity,
                            jerseyNumber: number.input.value === '' ? null : Number(number.input.value), printName: name.input.value.trim() || null });
                        cart.save(items);
                        status.textContent = `${product.teamName} sepete eklendi. Sepet bağlantısından devam edebilirsiniz.`;
                    } catch (error) {
                        status.textContent = error.message || 'Sepete erişilemiyor. Tarayıcı depolama iznini kontrol edin.';
                    }
                });
                card.append(emblem, title, amount, stock, form);
                grid.append(card);
            }
            status.textContent = products.length ? `${products.length} ürün listeleniyor.` : 'Koleksiyonda henüz ürün bulunmuyor.';
        } catch {
            status.textContent = 'Ürünler şu anda yüklenemiyor. Lütfen tekrar deneyin.';
        } finally {
            reload.disabled = false;
            grid.setAttribute('aria-busy', 'false');
        }
    }
    reload.addEventListener('click', load);
    load();
})();
