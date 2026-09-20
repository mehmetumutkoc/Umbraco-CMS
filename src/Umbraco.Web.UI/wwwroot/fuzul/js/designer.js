(function () {
    'use strict';
    const root = document.getElementById('fv-designer');
    if (!root) return;

    const statusEl = root.querySelector('[data-fv-status]');
    const inspector = root.querySelector('.fv-inspector');
    const dataNode = document.getElementById('fv-layout-data');
    const handles = 'nw,n,ne,e,se,s,sw,w'.split(',');
    const defaults = {
        button: { w: 18, h: 9, text: 'Buton', bg: '#64cdd2', color: '#081d34', size: 16, radius: 40 },
        text: { w: 28, h: 12, text: 'Yeni metin', color: '#8fa8bd', size: 16 },
        heading: { w: 40, h: 12, text: 'Başlık', color: '#ffffff', size: 36 },
        image: { w: 24, h: 24, src: '/fuzul/images/vent.jpg' },
        shape: { w: 18, h: 18, bg: '#102b44', radius: 8 }
    };

    let layout = parseLayout(dataNode ? dataNode.textContent : '{}');
    let history = [];
    let selected = null;
    let dirty = false;

    if (new URLSearchParams(location.search).get('duzenle') === '1') setEditing(true);

    document.querySelectorAll('[data-fv-toggle]').forEach(btn => btn.addEventListener('click', () => setEditing(!document.body.classList.contains('fv-designing'))));
    root.querySelectorAll('[data-fv-add]').forEach(btn => btn.addEventListener('click', () => addWidget(btn.getAttribute('data-fv-add'))));
    root.querySelector('[data-fv-save]').addEventListener('click', save);
    root.querySelector('[data-fv-undo]').addEventListener('click', undo);
    root.querySelector('[data-fv-delete]').addEventListener('click', removeSelected);
    inspector.querySelectorAll('[data-fv-field]').forEach(input => {
        input.addEventListener('focus', snapshot);
        input.addEventListener('input', () => applyField(input.getAttribute('data-fv-field'), input.value));
    });

    document.addEventListener('pointerdown', onPointerDown, true);
    document.addEventListener('keydown', onKey);
    window.addEventListener('beforeunload', event => {
        if (dirty) event.preventDefault();
    });

    function parseLayout(raw) {
        try {
            const data = JSON.parse(raw || '{}');
            return { v: 1, widgets: Array.isArray(data.widgets) ? data.widgets : [], pins: data.pins && typeof data.pins === 'object' ? data.pins : {} };
        } catch {
            return { v: 1, widgets: [], pins: {} };
        }
    }

    function snapshot() {
        history.push(JSON.stringify(layout));
        if (history.length > 30) history.shift();
        dirty = true;
        setStatus('Kaydedilmedi');
    }

    function setStatus(text) {
        if (statusEl) statusEl.textContent = text || '';
    }

    function setEditing(on) {
        document.body.classList.toggle('fv-designing', on);
        document.body.classList.toggle('fv-no-cursor', on);
        if (on) {
            document.querySelectorAll('[data-aos]').forEach(el => { el.classList.add('aos-animate'); });
            select(null);
            setStatus('Öğeyi sürükleyin veya soldan ekleyin');
        } else {
            select(null);
        }
    }

    function currentStage() {
        const stages = [...document.querySelectorAll('[data-fv-stage]')];
        const mid = window.innerHeight * 0.4;
        return stages.find(stage => {
            const box = stage.getBoundingClientRect();
            return box.top <= mid && box.bottom >= mid;
        }) || stages[0];
    }

    function stageOf(el) {
        return el.closest('[data-fv-stage]');
    }

    function uid(prefix) {
        return prefix + '-' + Math.random().toString(36).slice(2, 10);
    }

    function layerFor(stage) {
        let layer = stage.querySelector(':scope > .fv-widget-layer');
        if (!layer) {
            layer = document.createElement('div');
            layer.className = 'fv-widget-layer';
            stage.appendChild(layer);
        }
        return layer;
    }

    function addWidget(type) {
        if (!document.body.classList.contains('fv-designing')) setEditing(true);
        const stage = currentStage();
        if (!stage) return;
        snapshot();
        const preset = defaults[type] || defaults.button;
        const widget = Object.assign({
            id: uid('w'),
            section: stage.getAttribute('data-fv-stage'),
            type,
            x: 8,
            y: 18,
            z: 6,
            text: '',
            href: type === 'button' ? '#kurumsal' : '',
            bg: '',
            color: '',
            size: 16,
            radius: 0,
            src: ''
        }, preset);
        layout.widgets.push(widget);
        const el = renderWidget(widget);
        layerFor(stage).appendChild(el);
        select({ kind: 'widget', id: widget.id, el });
        setStatus('Sürükleyerek konumlandırın');
    }

    function renderWidget(widget) {
        const existing = document.querySelector('[data-fv-widget="' + widget.id + '"]');
        const el = existing || document.createElement(widget.type === 'button' && widget.href ? 'a' : 'div');
        el.className = 'fv-widget fv-widget--' + widget.type;
        el.setAttribute('data-fv-widget', widget.id);
        applyWidgetStyle(el, widget);
        if (widget.type === 'image') {
            el.innerHTML = '<img alt="">';
            el.querySelector('img').src = widget.src || '';
        } else if (widget.type === 'heading') {
            el.innerHTML = '<h2></h2>';
            el.querySelector('h2').textContent = widget.text || '';
        } else if (widget.type === 'text') {
            el.innerHTML = '<p></p>';
            el.querySelector('p').textContent = widget.text || '';
        } else if (widget.type === 'shape') {
            el.textContent = '';
        } else {
            el.textContent = widget.text || 'Buton';
            if (el.tagName === 'A') el.setAttribute('href', widget.href || '#');
        }
        return el;
    }

    function applyWidgetStyle(el, widget) {
        el.style.left = widget.x + '%';
        el.style.top = widget.y + '%';
        el.style.width = widget.w + '%';
        el.style.height = widget.h + '%';
        el.style.zIndex = String(widget.z || 4);
        el.style.background = widget.bg || '';
        el.style.color = widget.color || '';
        el.style.fontSize = (widget.size || 16) + 'px';
        el.style.borderRadius = (widget.radius || 0) + 'px';
    }

    function widgetById(id) {
        return layout.widgets.find(item => item.id === id);
    }

    function select(next) {
        document.querySelectorAll('.fv-selected').forEach(el => el.classList.remove('fv-selected'));
        document.querySelectorAll('.fv-handles').forEach(el => el.remove());
        selected = next;
        if (!next) {
            inspector.hidden = true;
            return;
        }
        next.el.classList.add('fv-selected');
        const box = document.createElement('div');
        box.className = 'fv-handles';
        handles.forEach(name => {
            const handle = document.createElement('span');
            handle.className = 'fv-handle';
            handle.dataset.handle = name;
            box.appendChild(handle);
        });
        next.el.appendChild(box);
        fillInspector();
        inspector.hidden = false;
    }

    function fillInspector() {
        const data = selectedData();
        if (!data) return;
        inspector.querySelector('[data-fv-field="text"]').value = data.text || selected.el.textContent.trim();
        inspector.querySelector('[data-fv-field="href"]').value = data.href || selected.el.getAttribute('href') || '';
        inspector.querySelector('[data-fv-field="src"]').value = data.src || '';
        inspector.querySelector('[data-fv-field="bg"]').value = toColor(data.bg || '#64cdd2');
        inspector.querySelector('[data-fv-field="color"]').value = toColor(data.color || '#ffffff');
        inspector.querySelector('[data-fv-field="size"]').value = String(data.size || 16);
        inspector.querySelector('[data-fv-field="radius"]').value = String(data.radius || 0);
        inspector.querySelector('[data-fv-delete]').hidden = selected.kind !== 'widget';
    }

    function selectedData() {
        if (!selected) return null;
        if (selected.kind === 'widget') return widgetById(selected.id);
        layout.pins[selected.id] = layout.pins[selected.id] || { section: stageOf(selected.el)?.getAttribute('data-fv-stage') || '', x: 0, y: 0, w: 0, h: 0, z: 5 };
        const pin = layout.pins[selected.id];
        pin.text = selected.el.textContent.trim();
        pin.href = selected.el.getAttribute('href') || '';
        return pin;
    }

    function toColor(value) {
        return /^#[0-9a-fA-F]{6}$/.test(value) ? value : '#64cdd2';
    }

    function applyField(field, value) {
        if (!selected) return;
        if (selected.kind === 'widget') {
            const widget = widgetById(selected.id);
            if (!widget) return;
            widget[field] = field === 'size' || field === 'radius' ? Number(value) : value;
            renderWidget(widget);
            selected.el = document.querySelector('[data-fv-widget="' + widget.id + '"]') || selected.el;
            selected.el.classList.add('fv-selected');
            return;
        }
        if (field === 'text') selected.el.childNodes.forEach(node => { if (node.nodeType === 3 || node.tagName === 'SPAN') node.textContent = value; });
        if (field === 'href' && selected.el.hasAttribute('href')) selected.el.setAttribute('href', value);
        if (field === 'bg') selected.el.style.background = value;
        if (field === 'color') selected.el.style.color = value;
        if (field === 'size') selected.el.style.fontSize = value + 'px';
        if (field === 'radius') selected.el.style.borderRadius = value + 'px';
    }

    function removeSelected() {
        if (!selected || selected.kind !== 'widget') return;
        snapshot();
        layout.widgets = layout.widgets.filter(item => item.id !== selected.id);
        selected.el.remove();
        select(null);
    }

    function percent(value, total) {
        if (!total) return 0;
        return Math.round((value / total) * 10000) / 100;
    }

    function pinNative(el) {
        const id = el.getAttribute('data-fv-pin');
        const stage = stageOf(el);
        if (!id || !stage) return layout.pins[id];
        if (layout.pins[id]) return layout.pins[id];
        const stageBox = stage.getBoundingClientRect();
        const box = el.getBoundingClientRect();
        const pin = {
            section: stage.getAttribute('data-fv-stage'),
            x: percent(box.left - stageBox.left, stageBox.width),
            y: percent(box.top - stageBox.top, stageBox.height),
            w: percent(box.width, stageBox.width),
            h: percent(box.height, stageBox.height),
            z: 5
        };
        layout.pins[id] = pin;
        applyPin(el, pin);
        return pin;
    }

    function applyPin(el, pin) {
        el.style.position = 'absolute';
        el.style.left = pin.x + '%';
        el.style.top = pin.y + '%';
        if (pin.w) el.style.width = pin.w + '%';
        if (pin.h) el.style.height = pin.h + '%';
        el.style.margin = '0';
        el.style.zIndex = String(pin.z || 5);
    }

    function onPointerDown(event) {
        if (!document.body.classList.contains('fv-designing')) return;
        if (event.target.closest('#fv-designer')) return;
        if (event.button !== 0) return;
        const handle = event.target.closest('.fv-handle');
        const widgetEl = event.target.closest('[data-fv-widget]');
        const pinEl = event.target.closest('[data-fv-pin]');
        const target = widgetEl || pinEl;
        if (!target) {
            select(null);
            return;
        }
        event.preventDefault();
        event.stopPropagation();
        if (widgetEl) select({ kind: 'widget', id: widgetEl.getAttribute('data-fv-widget'), el: widgetEl });
        else {
            snapshot();
            pinNative(pinEl);
            select({ kind: 'pin', id: pinEl.getAttribute('data-fv-pin'), el: pinEl });
        }
        const stage = stageOf(target);
        if (!stage) return;
        const start = { x: event.clientX, y: event.clientY };
        const box = target.getBoundingClientRect();
        const stageBox = stage.getBoundingClientRect();
        const origin = { x: box.left - stageBox.left, y: box.top - stageBox.top, w: box.width, h: box.height };
        const mode = handle ? handle.dataset.handle : 'move';
        snapshot();
        const move = ev => {
            const dx = ev.clientX - start.x;
            const dy = ev.clientY - start.y;
            let x = origin.x, y = origin.y, w = origin.w, h = origin.h;
            if (mode === 'move') { x += dx; y += dy; }
            if (mode.includes('e')) w = Math.max(24, origin.w + dx);
            if (mode.includes('s')) h = Math.max(24, origin.h + dy);
            if (mode.includes('w')) { w = Math.max(24, origin.w - dx); x = origin.x + dx; }
            if (mode.includes('n')) { h = Math.max(24, origin.h - dy); y = origin.y + dy; }
            const next = {
                x: percent(x, stageBox.width),
                y: percent(y, stageBox.height),
                w: percent(w, stageBox.width),
                h: percent(h, stageBox.height)
            };
            writeGeometry(next);
        };
        const up = () => {
            document.removeEventListener('pointermove', move);
            document.removeEventListener('pointerup', up);
        };
        document.addEventListener('pointermove', move);
        document.addEventListener('pointerup', up);
    }

    function writeGeometry(next) {
        if (!selected) return;
        if (selected.kind === 'widget') {
            const widget = widgetById(selected.id);
            if (!widget) return;
            Object.assign(widget, next);
            applyWidgetStyle(selected.el, widget);
            return;
        }
        const pin = layout.pins[selected.id];
        if (!pin) return;
        Object.assign(pin, next);
        applyPin(selected.el, pin);
    }

    function onKey(event) {
        if (!document.body.classList.contains('fv-designing')) return;
        if ((event.ctrlKey || event.metaKey) && event.key.toLowerCase() === 's') {
            event.preventDefault();
            save();
        }
        if ((event.ctrlKey || event.metaKey) && event.key.toLowerCase() === 'z') {
            event.preventDefault();
            undo();
        }
        if (event.key === 'Delete' || event.key === 'Backspace') {
            if (event.target.matches('input,textarea')) return;
            event.preventDefault();
            removeSelected();
        }
        if (event.key === 'Escape') select(null);
        if (selected && ['ArrowLeft', 'ArrowRight', 'ArrowUp', 'ArrowDown'].includes(event.key)) {
            event.preventDefault();
            snapshot();
            const delta = event.shiftKey ? 2 : 0.5;
            const geom = selected.kind === 'widget' ? widgetById(selected.id) : layout.pins[selected.id];
            if (!geom) return;
            if (event.key === 'ArrowLeft') geom.x -= delta;
            if (event.key === 'ArrowRight') geom.x += delta;
            if (event.key === 'ArrowUp') geom.y -= delta;
            if (event.key === 'ArrowDown') geom.y += delta;
            writeGeometry(geom);
        }
    }

    function undo() {
        const prev = history.pop();
        if (!prev) return;
        const oldPins = Object.keys(layout.pins);
        layout = parseLayout(prev);
        document.querySelectorAll('[data-fv-widget]').forEach(el => el.remove());
        layout.widgets.forEach(widget => {
            const stage = document.querySelector('[data-fv-stage="' + widget.section + '"]');
            if (stage) layerFor(stage).appendChild(renderWidget(widget));
        });
        oldPins.forEach(id => {
            if (layout.pins[id]) return;
            const el = document.querySelector('[data-fv-pin="' + id + '"]');
            if (el) ['position', 'left', 'top', 'width', 'height', 'margin', 'zIndex'].forEach(prop => { el.style[prop] = ''; });
        });
        Object.keys(layout.pins).forEach(id => {
            const el = document.querySelector('[data-fv-pin="' + id + '"]');
            if (el) applyPin(el, layout.pins[id]);
        });
        select(null);
        setStatus('Geri alındı');
    }

    async function save() {
        setStatus('Kaydediliyor…');
        const body = new URLSearchParams();
        body.set('pageKey', root.dataset.page);
        body.set('layout', JSON.stringify(layout));
        body.set('__RequestVerificationToken', root.dataset.token);
        try {
            const response = await fetch(root.dataset.save, {
                method: 'POST',
                headers: { 'Content-Type': 'application/x-www-form-urlencoded;charset=UTF-8', RequestVerificationToken: root.dataset.token },
                body,
                credentials: 'same-origin'
            });
            const data = await response.json().catch(() => ({ ok: false, error: 'Yanıt okunamadı' }));
            if (!response.ok || !data.ok) {
                setStatus(data.error || 'Kayıt başarısız');
                return;
            }
            dirty = false;
            if (data.layout) layout = parseLayout(data.layout);
            setStatus('Kaydedildi');
        } catch {
            setStatus('Kayıt başarısız');
        }
    }
})();
