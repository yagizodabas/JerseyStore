(() => {
    const key = 'cart';
    function read() {
        const items = JSON.parse(localStorage.getItem(key) || '[]');
        if (!Array.isArray(items) || items.length > 100 || items.some(item => !item ||
            !Number.isInteger(item.productId) || item.productId < 1 ||
            !Number.isInteger(item.quantity) || item.quantity < 1 || item.quantity > 100 ||
            typeof item.teamName !== 'string' || !Number.isFinite(item.price) || item.price < 0 ||
            (item.jerseyNumber != null && (!Number.isInteger(item.jerseyNumber) || item.jerseyNumber < 0 || item.jerseyNumber > 99)) ||
            (item.printName != null && (typeof item.printName !== 'string' || item.printName.length > 40)))) {
            throw new Error('Sepet verisi okunamıyor. Sepeti temizleyip ürünleri yeniden ekleyebilirsiniz.');
        }
        return items;
    }
    window.JerseyCart = {
        read,
        save(items) { localStorage.setItem(key, JSON.stringify(items)); },
        clear() { localStorage.removeItem(key); },
        money(value) { return new Intl.NumberFormat('tr-TR', { style: 'currency', currency: 'TRY' }).format(value); }
    };
})();
