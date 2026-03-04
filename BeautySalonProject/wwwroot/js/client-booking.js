(function () {
    const root = document.getElementById("bookFormRoot");
    if (!root) return;

    const variantId = root.getAttribute("data-variant-id");
    const dateInput = document.getElementById("dateInput");
    const slotsBox = document.getElementById("slotsBox");     // div container
    const slotsHint = document.getElementById("slotsHint");

    const sumDate = document.getElementById("sumDate");
    const sumTime = document.getElementById("sumTime");

    if (!slotsBox) return;

    function getSelectedEmployeeId() {
        const checked = document.querySelector('input[name="EmployeeId"]:checked');
        return checked ? checked.value : null;
    }

    function setHint(text) {
        if (slotsHint) slotsHint.textContent = text || "";
    }

    function setLoadingState(isLoading) {
        if (isLoading) {
            slotsBox.innerHTML = `<div class="sh-muted small">Зареждам наличните часове…</div>`;
            setHint("");
        }
    }

    function updateSummaryDate() {
        if (sumDate && dateInput) sumDate.textContent = dateInput.value || "—";
    }

    function updateSummaryTime(value) {
        if (sumTime) sumTime.textContent = value || "—";
    }

    function renderSlots(slots) {
        // slots идват само налични (IsAvailable=true) от API-то при теб
        if (!slots || slots.length === 0) {
            slotsBox.innerHTML = `<div class="sh-muted small">Няма налични часове за тази дата.</div>`;
            setHint("Няма налични часове за тази дата.");
            updateSummaryTime("");
            return;
        }

        // Radio buttons grid
        const html = `
          <div class="sh-slots-grid">
            ${slots.map(s => {
            const safeId = "slot_" + String(s.value).replace(":", "");
            return `
                  <input class="btn-check" type="radio" name="StartTime" id="${safeId}" value="${s.value}">
                  <label class="sh-slot" for="${safeId}">${s.label}</label>
                `;
        }).join("")}
          </div>
        `;

        slotsBox.innerHTML = html;
        setHint(`Налични часове: ${slots.length}`);

        // hook change event to update summary
        slotsBox.querySelectorAll('input[name="StartTime"]').forEach(r => {
            r.addEventListener("change", () => updateSummaryTime(r.value));
        });
    }

    async function loadSlots() {
        const employeeId = getSelectedEmployeeId();
        const date = dateInput ? dateInput.value : null;

        updateSummaryDate();

        if (!employeeId || !date || !variantId) {
            slotsBox.innerHTML = `<div class="sh-muted small">Избери служител и дата, за да заредим наличните часове…</div>`;
            setHint("");
            updateSummaryTime("");
            return;
        }

        setLoadingState(true);

        try {
            const url = new URL("/Client/Appointments/Slots", window.location.origin);
            url.searchParams.set("employeeId", employeeId);
            url.searchParams.set("date", date);
            url.searchParams.set("variantId", variantId);

            const resp = await fetch(url.toString(), {
                headers: { "Accept": "application/json" }
            });

            if (!resp.ok) {
                slotsBox.innerHTML = `<div class="text-danger small">Грешка при зареждане на часовете (${resp.status}).</div>`;
                setHint("Грешка при зареждане на часовете.");
                updateSummaryTime("");
                return;
            }

            const data = await resp.json();
            renderSlots(data);
        } catch (e) {
            slotsBox.innerHTML = `<div class="text-danger small">Грешка при връзка със сървъра.</div>`;
            setHint("Грешка при връзка със сървъра.");
            updateSummaryTime("");
        }
    }

    // Events
    document.querySelectorAll('input[name="EmployeeId"]').forEach(r => {
        r.addEventListener("change", loadSlots);
    });

    if (dateInput) dateInput.addEventListener("change", loadSlots);

    // initial
    loadSlots();
})();