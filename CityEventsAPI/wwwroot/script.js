const apiUrl = "/api/ticketorders";
const lookupApiUrl = "/api/lookup";

async function fetchTicketOrders() {
    const response = await fetch(apiUrl);
    const orders = await response.json();
    console.log(orders);
    const tableBody = document.getElementById("ticketOrdersTableBody");
    tableBody.innerHTML = "";
    orders.forEach(order => {
        const row = document.createElement("tr");
        row.innerHTML = `
            <td>${order.orderID}</td>
            <td>${order.customerName}</td>
            <td>${order.eventName}</td>
            <td>${new Date(order.orderDate).toLocaleDateString()}</td>
            <td>${order.ticketCount}</td>
            <td>
                <button class="btn btn-sm btn-primary" onclick="editOrder(${order.orderID})">Редактировать</button>
                <button class="btn btn-sm btn-danger" onclick="deleteOrder(${order.orderID})">Удалить</button>
            </td>
        `;
        tableBody.appendChild(row);
    });
}

async function loadEvents(firstLimit = 5000, lastLimit = 100) {
    const response = await fetch(`${apiUrl}/events?firstLimit=${firstLimit}&lastLimit=${lastLimit}`);
    const events = await response.json();
    const eventSelect = document.getElementById("eventId");
    eventSelect.innerHTML = "";

    events.forEach(event => {
        const option = document.createElement("option");
        option.value = event.eventID;
        option.textContent = event.eventName;
        eventSelect.appendChild(option);
    });
}

async function loadCustomers() {
    const response = await fetch(`${apiUrl}/customers`);
    const customers = await response.json();
    const customerSelect = document.getElementById("customerId");
    customerSelect.innerHTML = "";

    customers.forEach(customer => {
        const option = document.createElement("option");
        option.value = customer.customerID;
        option.textContent = customer.fullName;
        customerSelect.appendChild(option);
    });
}

async function editOrder(orderId) {
    const response = await fetch(`${apiUrl}/${orderId}`);
    const order = await response.json();

    document.getElementById("orderId").value = order.orderID;

    await loadCustomers();
    document.getElementById("customerId").value = order.customerID;

    await loadEvents();
    document.getElementById("eventId").value = order.eventID;

    document.getElementById("orderDate").value = new Date(order.orderDate).toISOString().split("T")[0];
    document.getElementById("ticketCount").value = order.ticketCount;
}

async function saveOrder(event) {
    event.preventDefault();

    const orderId = document.getElementById("orderId").value;
    const customerId = document.getElementById("customerId").value;
    const eventId = document.getElementById("eventId").value;
    const orderDate = document.getElementById("orderDate").value;
    const ticketCount = document.getElementById("ticketCount").value;

    const order = {
        orderID: orderId ? parseInt(orderId) : 0,
        customerID: parseInt(customerId),
        eventID: parseInt(eventId),
        orderDate,
        ticketCount: parseInt(ticketCount)
    };

    const method = orderId == 0 ? "POST" : "PUT";
    const url = orderId == 0 ? apiUrl : `${apiUrl}/${orderId}`;

    await fetch(url, {
        method,
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(order)
    });

    resetForm();
    fetchTicketOrders();
}

async function deleteOrder(orderId) {
    if (confirm("Вы действительно хотите удалить?")) {
        await fetch(`${apiUrl}/${orderId}`, { method: "DELETE" });
        fetchTicketOrders();
    }
}

async function searchTicketOrders(event) {
    event.preventDefault();

    const eventName = document.getElementById("searchEventName").value.trim();
    if (eventName === "") {
        fetchTicketOrders();
        return;
    }

    const response = await fetch(`${apiUrl}/search?eventName=${encodeURIComponent(eventName)}`);
    const orders = await response.json();
    populateTable(orders);
}

function populateTable(orders) {
    const tableBody = document.getElementById("ticketOrdersTableBody");
    tableBody.innerHTML = "";
    orders.forEach(order => {
        const row = document.createElement("tr");
        row.innerHTML = `
            <td>${order.orderID}</td>
            <td>${order.customerName}</td>
            <td>${order.eventName}</td>
            <td>${new Date(order.orderDate).toLocaleDateString()}</td>
            <td>${order.ticketCount}</td>
            <td>
                <button class="btn btn-sm btn-primary" onclick="editOrder(${order.orderID})">Редактировать</button>
                <button class="btn btn-sm btn-danger" onclick="deleteOrder(${order.orderID})">Удалить</button>
            </td>
        `;
        tableBody.appendChild(row);
    });
}

function resetForm() {
    document.getElementById("orderId").value = 0;
    document.getElementById("customerId").value = "";
    document.getElementById("eventId").value = "";
    document.getElementById("orderDate").value = "";
    document.getElementById("ticketCount").value = "";
}

document.getElementById("searchForm").addEventListener("submit", searchTicketOrders);

document.getElementById("ticketOrderForm").addEventListener("submit", saveOrder);

document.addEventListener("DOMContentLoaded", () => {
    loadCustomers();
    loadEvents();
    fetchTicketOrders();
});