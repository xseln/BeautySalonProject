async function loadSlots() {

    const date = document.querySelector("[name=date]").value
    const employee = document.querySelector("[name=employeeId]").value
    const variantId = document.querySelector("[name=variantId]").value 

    if (!date || !employee || !variantId) return

    const response = await fetch(`/Client/Appointments/GetAvailableSlots?date=${date}&employeeId=${employee}&variantId=${variantId}`)

    const slots = await response.json()

    const box = document.getElementById("slotsBox")

    box.innerHTML = ""

    if (slots.length === 0) {
        box.innerHTML = `<span class="text-muted">Няма свободни часове</span>`
        return
    }

    slots.forEach(s => {

        const slot = document.createElement("label")

        slot.className = "sh-slot"

        slot.innerHTML = `
            <input type="radio" name="slot" value="${s}" hidden>
            ${s}
        `
        slot.onclick = () => {
            document.querySelectorAll(".sh-slot").forEach(el => el.classList.remove("active"))
            slot.classList.add("active")

            slot.querySelector("input").checked = true
        }
        box.appendChild(slot)
    })
}

document.addEventListener("DOMContentLoaded", () => {

    console.log("EDIT JS LOADED")

    const dateInput = document.querySelector("[name=date]")
    const employeeInput = document.querySelector("[name=employeeId]")

    if (dateInput)
        dateInput.addEventListener("change", loadSlots)

    if (employeeInput)
        employeeInput.addEventListener("change", loadSlots)

    loadSlots()
})