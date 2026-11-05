(() => {
    const cart = window.JerseyCart;
    const tbody = document.querySelector('#cart-body');
    const status = document.querySelector('#cart-status');
    const total = document.querySelector('#cart-total');
    const form = document.querySelector('#checkout');
    const submit = document.querySelector('#submit-order');
    const clear = document.querySelector('#clear-cart');
    const result = document.querySelector('#order-result');
    let busy = false;
    let uncertain = false;
    function render() {
        tbody.replaceChildren();
        submit.disabled = true;
        total.textContent = cart.money(0);
        try {
            const items = cart.read();
            let amount = 0;
            for (const [index, item] of items.entries()) {
                const row = document.createElement('tr');
                const subtotal = item.price * item.quantity;
                amount += subtotal;
                for (const value of [item.teamName, item.quantity, item.jerseyNumber ?? '—', item.printName ?? '—', cart.money(subtotal)]) {
                    const cell = document.createElement('td');
                    cell.textContent = value;
                    row.append(cell);
                }
                const cell = document.createElement('td');
                const remove = document.createElement('button');
                remove.className = 'btn btn-sm btn-outline-danger';
                remove.type = 'button';
                remove.textContent = 'Çıkar';
                remove.setAttribute('aria-label', `${item.teamName} ürününü sepetten çıkar`);
                remove.disabled = busy || uncertain;
                remove.addEventListener('click', () => {
                    try {
                        const current = cart.read();
                        current.splice(index, 1);
                        cart.save(current);
                        render();
                    } catch { status.textContent = 'Sepet değiştirilemedi. Tarayıcı depolama iznini kontrol edin.'; }
                });
                cell.append(remove);
                row.append(cell);
                tbody.append(row);
            }
            total.textContent = cart.money(amount);
            status.textContent = items.length ? `${items.length} ürün satırı.` : 'Sepetiniz boş. Koleksiyondan ürün ekleyebilirsiniz.';
            submit.disabled = !items.length || busy || uncertain;
        } catch (error) { status.textContent = error.message || 'Sepete erişilemiyor.'; }
    }
    clear.addEventListener('click', () => {
        try { cart.clear(); render(); }
        catch { status.textContent = 'Sepet temizlenemedi.'; }
    });
    form.addEventListener('submit', async event => {
        event.preventDefault();
        if (busy || uncertain || !form.reportValidity()) return;
        let items;
        try { items = cart.read(); }
        catch { status.textContent = 'Sepet okunamıyor. Gönderim yapılmadı.'; return; }
        if (!items.length) return;
        busy = true;
        clear.disabled = true;
        result.textContent = 'Sipariş gönderiliyor…';
        render();
        try {
            const response = await fetch('/api/store/order', {
                method: 'POST', headers: { 'Content-Type': 'application/json' },
                signal: AbortSignal.timeout(30000),
                body: JSON.stringify({ customerName: form.elements.customerName.value.trim(), address: form.elements.address.value.trim(),
                    phone: form.elements.phone.value.trim(), items: items.map(({ productId, quantity, jerseyNumber, printName }) => ({ productId, quantity, jerseyNumber, printName })) })
            });
            const data = await response.json();
            if (response.ok && data.success) {
                // Prevent a duplicate order even if clearing browser storage fails.
                uncertain = true;
                result.textContent = `Sipariş alındı. Numara: ${data.orderNumber}. Toplam: ${cart.money(data.totalAmount)}.`;
                try { cart.clear(); } catch { result.textContent += ' Sepet tarayıcıdan temizlenemedi; aynı siparişi yeniden göndermeyin.'; }
                form.reset();
            } else if (response.status >= 500) {
                uncertain = true;
                result.textContent = 'Sipariş sonucu doğrulanamadı. Çift sipariş oluşturmamak için tekrar göndermeden önce mağaza ile kontrol edin.';
            } else {
                result.textContent = data.message || Object.values(data.errors || {}).flat().join(' ') || 'Sipariş reddedildi. Bilgileri ve stokları kontrol edin.';
            }
        } catch {
            uncertain = true;
            result.textContent = 'Sunucu yanıtı alınamadı. Sipariş kaydedilmiş olabilir; çift sipariş oluşturmamak için yeniden göndermeden önce mağaza ile kontrol edin.';
        } finally {
            busy = false;
            clear.disabled = uncertain;
            render();
        }
    });
    render();
})();
