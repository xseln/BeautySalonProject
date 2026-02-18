(function () {
    function serviceTemplate(i, categorySelectHtml) {
        return `
<div class="border rounded p-3 mb-3 service-block" data-service-index="${i}">
  <input type="hidden" name="Services[${i}].ServiceId" value="" />

  <div class="row g-2 align-items-end">
    <div class="col-md-4">
      <label class="form-label">Категория</label>
      <select class="form-select" name="Services[${i}].CategoryId">${categorySelectHtml}</select>
    </div>

    <div class="col-md-5">
      <label class="form-label">Услуга</label>
      <input class="form-control" name="Services[${i}].Name" value="" />
    </div>

    <div class="col-md-2">
      <label class="form-label">Активна</label><br />
      <input class="form-check-input" type="checkbox" name="Services[${i}].IsActive" value="true" checked />
      <input type="hidden" name="Services[${i}].IsActive" value="false" />
    </div>

    <div class="col-md-1 text-end">
      <button type="button" class="btn btn-sm btn-outline-danger remove-service">×</button>
    </div>
  </div>

  <div class="mt-3 d-flex justify-content-between align-items-center">
    <div class="fw-semibold">Варианти</div>
    <button type="button" class="btn btn-sm btn-outline-secondary add-variant">+ Вариант</button>
  </div>

  <div class="mt-2 variants-wrap">
    ${variantTemplate(i, 0)}
  </div>
</div>`;
    }

    function variantTemplate(i, j) {
        return `
<div class="row g-2 align-items-end mb-2 variant-row" data-variant-index="${j}">
  <input type="hidden" name="Services[${i}].Variants[${j}].VariantId" value="" />

  <div class="col-md-4">
    <label class="form-label">Име</label>
    <input class="form-control" name="Services[${i}].Variants[${j}].VariantName" value="" />
  </div>

  <div class="col-md-3">
    <label class="form-label">Цена</label>
    <input class="form-control" name="Services[${i}].Variants[${j}].Price" value="0" />
  </div>

  <div class="col-md-3">
    <label class="form-label">Минути</label>
    <input class="form-control" name="Services[${i}].Variants[${j}].DurationMinutes" value="60" />
  </div>

  <div class="col-md-1">
    <label class="form-label">Активен</label><br />
    <input class="form-check-input" type="checkbox" name="Services[${i}].Variants[${j}].IsActive" value="true" checked />
    <input type="hidden" name="Services[${i}].Variants[${j}].IsActive" value="false" />
  </div>

  <div class="col-md-1 text-end">
    <button type="button" class="btn btn-sm btn-outline-danger remove-variant">×</button>
  </div>
</div>`;
    }

    function reindexAll() {
        const services = document.querySelectorAll(".service-block");
        services.forEach((sb, i) => {
            sb.setAttribute("data-service-index", i);

            // update names inside service
            sb.querySelectorAll("[name]").forEach(el => {
                el.name = el.name
                    .replace(/Services\[\d+\]/g, `Services[${i}]`)
                    .replace(/Services\[\d+\]\.*/g, el.name.replace(/Services\[\d+\]/, `Services[${i}]`));
            });

            // variants
            const variants = sb.querySelectorAll(".variant-row");
            variants.forEach((vr, j) => {
                vr.setAttribute("data-variant-index", j);
                vr.querySelectorAll("[name]").forEach(el => {
                    el.name = el.name
                        .replace(/Services\[\d+\]/g, `Services[${i}]`)
                        .replace(/Variants\[\d+\]/g, `Variants[${j}]`);
                });
            });
        });
    }

    function initEmployeeServices() {
        const wrap = document.getElementById("servicesWrap");
        const addServiceBtn = document.getElementById("addServiceBtn");

        if (!wrap || !addServiceBtn) return;

        // взимаме опциите за категориите от първия select
        const firstSelect = wrap.querySelector("select.form-select");
        const categorySelectHtml = firstSelect ? firstSelect.innerHTML : "";

        addServiceBtn.addEventListener("click", () => {
            const i = wrap.querySelectorAll(".service-block").length;
            wrap.insertAdjacentHTML("beforeend", serviceTemplate(i, categorySelectHtml));
        });

        document.addEventListener("click", (e) => {
            const t = e.target;

            if (t.classList && t.classList.contains("remove-service")) {
                t.closest(".service-block")?.remove();
                reindexAll();
            }

            if (t.classList && t.classList.contains("add-variant")) {
                const sb = t.closest(".service-block");
                if (!sb) return;

                const i = parseInt(sb.getAttribute("data-service-index"), 10);
                const vw = sb.querySelector(".variants-wrap");
                if (!vw) return;

                const j = vw.querySelectorAll(".variant-row").length;
                vw.insertAdjacentHTML("beforeend", variantTemplate(i, j));
            }

            if (t.classList && t.classList.contains("remove-variant")) {
                t.closest(".variant-row")?.remove();
                reindexAll();
            }
        });
    }

    // auto-init
    if (document.readyState === "loading") {
        document.addEventListener("DOMContentLoaded", initEmployeeServices);
    } else {
        initEmployeeServices();
    }
})();
