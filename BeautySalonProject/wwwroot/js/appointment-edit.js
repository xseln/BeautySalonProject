async function loadSlots() {

    const date = document.querySelector("[name=date]").value
    const employee = document.querySelector("[name=employeeId]").value

    if (!date || !employee) return

    const response = await fetch(`/Client/Appointments/GetSlots?date=${date}&employeeId=${employee}`)

    const slots = await response.json()

    const box = document.getElementById("slotsBox")

    box.innerHTML = ""

    slots.forEach(s => {

        const slot = document.createElement("label")

        slot.className = "sh-slot"

        slot.innerHTML = `
            <input type="radio" name="slot" value="${s}" hidden>
            ${s}
        `

        box.appendChild(slot)

    })
}

document.addEventListener("DOMContentLoaded", () => {

    const dateInput = document.querySelector("[name=date]")
    const employeeInput = document.querySelector("[name=employeeId]")

    if (dateInput)
        dateInput.addEventListener("change", loadSlots)

    if (employeeInput)
        employeeInput.addEventListener("change", loadSlots)

    loadSlots()

})