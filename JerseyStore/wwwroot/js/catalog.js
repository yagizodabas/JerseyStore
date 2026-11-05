(() => {
    const grid = document.querySelector('#products');
    const status = document.querySelector('#catalog-status');
    const reload = document.querySelector('#reload-products');
    const price = new Intl.NumberFormat('tr-TR', { maximumFractionDigits: 2, minimumFractionDigits: 2 });
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
                // Currency is not defined by the source data; do not invent a currency symbol.
                amount.textContent = price.format(product.price);
                const stock = document.createElement('p');
                stock.className = 'product-stock';
                stock.textContent = product.stock > 0 ? `${product.stock} adet stokta` : 'Stok tükendi';
                card.append(emblem, title, amount, stock);
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
