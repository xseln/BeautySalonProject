document.addEventListener("DOMContentLoaded", function () {
    // Референции към елементите
    const variantSelect = document.getElementById("variantSelect");
    const dateInput = document.getElementById("dateInput");
    const slotsBox = document.getElementById("slotsBox");

    // ФУНКЦИЯ ЗА ЗАРЕЖДАНЕ НА ЧАСОВЕ
    window.loadAvailableSlots = async function () {
        const empInput = document.getElementById("employeeSelectHidden");
        const varSelect = document.getElementById("variantSelect");
        const dateInp = document.getElementById("dateInput");

        if (!empInput || !varSelect || !dateInp) {
            console.error("Критична грешка: Липсват елементи в HTML-а.");
            return;
        }

        const empId = empInput.value;
        const varId = varSelect.value;
        const date = dateInp.value;

        if (!empId || !varId || !date) {
            console.log("Изчаква се пълен избор (Специалист, Вариант и Дата).");
            return;
        }

        if (slotsBox) {
            slotsBox.innerHTML = '<div class="w-100 text-center py-2" style="color:#D4AF37">Проверка на графика...</div>';
        }

        try {
            const url = `/Client/Appointments/GetAvailableSlots?employeeId=${empId}&variantId=${varId}&date=${date}`;
            const response = await fetch(url);

            if (!response.ok) throw new Error("Server error");

            const slots = await response.json();

            if (slotsBox) {
                slotsBox.innerHTML = "";
                if (!slots || slots.length === 0) {
                    slotsBox.innerHTML = '<span class="text-danger small w-100 text-center">Няма свободни часове за избрания ден.</span>';
                    return;
                }

                slots.forEach(time => {
                    const btn = document.createElement("button");
                    btn.type = "button";
                    btn.className = "sh-slot-btn";
                    btn.style.cssText = "background: #ffffff; border: 1px solid #D4AF37; color: #4A3728; padding: 8px 18px; border-radius: 5px; cursor: pointer; margin: 2px;";
                    btn.textContent = time;

                    btn.onclick = function () {
                        document.querySelectorAll(".sh-slot-btn").forEach(b => {
                            b.style.backgroundColor = "#ffffff";
                            b.style.color = "#4A3728";
                        });
                        this.style.backgroundColor = "#D4AF37";
                        this.style.color = "#ffffff";

                        if (document.getElementById("finalStartTime")) {
                            document.getElementById("finalStartTime").value = time;
                        }
                        if (document.getElementById("infoTime")) {
                            document.getElementById("infoTime").textContent = time;
                        }
                    };
                    slotsBox.appendChild(btn);
                });
            }
        } catch (err) {
            console.error("Грешка при зареждане на часове:", err);
            if (slotsBox) slotsBox.innerHTML = '<span class="text-danger">Грешка при връзка.</span>';
        }
    };

    variantSelect?.addEventListener("change", function () {
        const selected = this.options[this.selectedIndex];
        if (this.value) {
            if (document.getElementById("infoServiceName")) document.getElementById("infoServiceName").textContent = selected.text;
            if (document.getElementById("infoPrice")) document.getElementById("infoPrice").textContent = selected.getAttribute("data-price");
            if (document.getElementById("finalVariantId")) document.getElementById("finalVariantId").value = this.value;
        }
        window.loadAvailableSlots();
    });

    dateInput?.addEventListener("change", function () {
        if (this.value) {
            if (document.getElementById("finalDate")) document.getElementById("finalDate").value = this.value;
            if (document.getElementById("infoDate")) {
                const dateObj = new Date(this.value);
                document.getElementById("infoDate").textContent = dateObj.toLocaleDateString('bg-BG');
            }
        }
        window.loadAvailableSlots();
    });

    document.getElementById("categorySelect")?.addEventListener("change", async function () {
        const svcSelect = document.getElementById("serviceSelect");
        if (!this.value || !svcSelect) return;
        const response = await fetch(`/Client/Appointments/GetServicesByCategory?categoryId=${this.value}`);
        const data = await response.json();
        svcSelect.innerHTML = '<option value="">-- избери услуга --</option>';
        data.forEach(s => svcSelect.innerHTML += `<option value="${s.serviceId}">${s.name}</option>`);
    });

    document.getElementById("serviceSelect")?.addEventListener("change", async function () {
        const varSelect = document.getElementById("variantSelect");
        if (!this.value || !varSelect) return;
        const response = await fetch(`/Client/Appointments/GetVariants?serviceId=${this.value}`);
        const data = await response.json();
        varSelect.innerHTML = '<option value="">-- избери вариант --</option>';
        data.forEach(v => varSelect.innerHTML += `<option value="${v.variantId}" data-price="${v.price}">${v.variantName}</option>`);
    });

});
