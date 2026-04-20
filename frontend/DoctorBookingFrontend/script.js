let allDoctors = []; // 🔥 store full list
// 🔐 LOGIN
function login() {
    const role = document.getElementById("role").value;

    let url = "";

    if (role === "user")
        url = "http://localhost:5066/api/user/login";
    else if (role === "doctor")
        url = "http://localhost:5066/api/doctor/login";
    else
        url = "http://localhost:5066/api/admin/login";

    fetch(url, {
        method: "POST",
        headers: {"Content-Type": "application/json"},
        body: JSON.stringify({
            email: email.value,
            password: password.value
        })
    })
    .then(res => res.json())
    .then(data => {
        console.log("LOGIN RESPONSE:", data); 
        localStorage.setItem("userId", data.id);
        localStorage.setItem("userName", data.name);

        if (role === "user")
            window.location.href = "patient-dashboard.html";
        else if (role === "doctor")
            window.location.href = "doctor-dashboard.html";
        else
            window.location.href = "admin-dashboard.html";
    });
}


// 📝 REGISTER
function register() {
    fetch("http://localhost:5066/api/user/register", {
        method: "POST",
        headers: {"Content-Type": "application/json"},
        body: JSON.stringify({
    name: document.getElementById("name").value,
    email: document.getElementById("email").value,
    password: document.getElementById("password").value,
    role: "PATIENT"
})
    })
    .then(() => {
        alert("Registered Successfully");
        window.location.href = "login.html";
    });
}


function viewDoctors() {

    fetch("http://localhost:5066/api/doctor/all")
    .then(res => res.json())
    .then(data => {

        // 🔥 Filter only APPROVED doctors
        allDoctors = data.filter(d => d.status === "APPROVED");

        // 🔥 render using reusable function
        renderDoctors(allDoctors);
    });
}

function renderDoctors(doctors) {

    let html = "";

    doctors.forEach(d => {

        let rating = "0.0";

        html += `
        <div class="card">

            <img src="${d.imageUrl}" 
                 onerror="this.src='https://via.placeholder.com/100'">

            <h3>${d.name}</h3>

            <p>${d.specialization}</p>

            <p>₹${d.fees}</p>

            <p class="rating" id="rating-${d.doctorId}">⭐ ${rating}</p>

            <input type="date" id="date-${d.doctorId}">

            <select id="time-${d.doctorId}">
                <option>10:00 AM</option>
                <option>11:00 AM</option>
                <option>12:00 PM</option>
                <option>02:00 PM</option>
            </select>

            <button onclick="book(${d.doctorId}, this)">Book</button>
            <button onclick="viewReviews(${d.doctorId})">View Reviews ⭐</button>
        </div>
        `;
    });

    document.getElementById("doctorList").innerHTML = html;

    // ⭐ Load ratings
    doctors.forEach(d => {
        fetch(`http://localhost:5066/api/review/average/${d.doctorId}`)
        .then(res => res.json())
        .then(avg => {
            document.getElementById(`rating-${d.doctorId}`).innerText =
                "⭐ " + avg.toFixed(1);
        });
    });
}
function searchDoctors() {

    const value = document
        .getElementById("searchBox")
        .value
        .toLowerCase();

    const filtered = allDoctors.filter(d =>
        d.specialization.toLowerCase().includes(value)
    );

    renderDoctors(filtered);
}

function book(doctorId, btn) {

    const date = document.getElementById(`date-${doctorId}`).value;
    const time = document.getElementById(`time-${doctorId}`).value;

    if (!date) {
        alert("Select date ❌");
        return;
    }

    if (btn) btn.disabled = true;

    fetch("http://localhost:5066/api/appointment/book", {
        method: "POST",
        headers: {"Content-Type": "application/json"},
        body: JSON.stringify({
            patientId: parseInt(localStorage.getItem("userId")),
            doctorId: doctorId,
            date: date,
            timeSlot: time
        })
    })
    .then(res => {
        if (!res.ok) throw new Error("Slot already booked");
        return res.text();
    })
    .then(() => {
        alert("Appointment Booked ✅");

        // 🔥 FIX HERE
        if (typeof viewAppointments === "function") {
            viewAppointments();
        }
    })
    .catch(() => {
        alert("Slot already booked ❌");
    })
    .finally(() => {
        if (btn) btn.disabled = false;
    });
}





// ❌ CANCEL
function cancel(id) {
    fetch(`http://localhost:5066/api/appointment/cancel/${id}`, {
        method: "PUT"
    })
    .then(() => {
        alert("Appointment Cancelled");
        viewAppointments();
    });
}


// 🚪 LOGOUT
function logout() {
    localStorage.clear();
    window.location.href = "login.html";
}

function viewDoctorAppointments() {
    const doctorId = localStorage.getItem("userId");

    fetch(`http://localhost:5066/api/appointment/doctor/${doctorId}`)
    .then(res => res.json())
    .then(data => {

        let html = "";

        data.forEach(a => {

            let action = "-";

            if (a.status === "BOOKED") {
                action = `
                    <button onclick="accept(${a.appointmentId})">Accept</button>
                    <button onclick="reject(${a.appointmentId})">Reject</button>
                `;
            }
            else if (a.status === "ACCEPTED") {
                action = `
                    <button onclick="complete(${a.appointmentId})">Complete</button>
                `;
            }
            else if (a.status === "COMPLETED") {
                action = `✔ Completed`;
            }

            html += `
                <tr>
                    <td>${new Date(a.date).toLocaleDateString()}</td>
                    <td>${a.timeSlot}</td>
                    <td>${a.patientName || a.patientId}</td>
                    <td>${a.status}</td>
                    <td>${action}</td>
                </tr>
            `;
        });

        document.getElementById("doctorAppointments").innerHTML = html;
    });
}

function accept(id) {
    fetch(`http://localhost:5066/api/appointment/accept/${id}`, {
        method: "PUT"
    })
    .then(() => {
        alert("Accepted ✅");
        viewDoctorAppointments();
    });
}

function reject(id) {
    fetch(`http://localhost:5066/api/appointment/reject/${id}`, {
        method: "PUT"
    })
    .then(() => {
        alert("Rejected ❌");
        viewDoctorAppointments();// refresh doctor view
    });
}
function loadDoctors() {
    fetch("http://localhost:5066/api/doctor/admin")
    .then(res => res.json())
    .then(data => {
        let html = "";

        data.forEach(d => {
            html += `
                <tr>
                    <td>${d.name}</td>
                    <td>${d.specialization}</td>
                    <td>${d.status}</td>
                    <td>
                        ${
                            d.status === "PENDING"
                            ? `<button onclick="approveDoctor(${d.doctorId})">Approve</button>`
                            : ""
                        }

                        ${
                            d.status === "APPROVED"
                            ? `<button onclick="removeDoctor(${d.doctorId})" style="background:red;color:white;">Remove</button>`
                            : ""
                        }
                    </td>
                </tr>
            `;
        });

        document.getElementById("doctorTable").innerHTML = html;
    });
}
function approveDoctor(id) {
    fetch(`http://localhost:5066/api/doctor/approve/${id}`, {
        method: "PUT"
    })
    .then(() => {
        alert("Doctor Approved ✅");
        loadDoctors();
    });
}
function loadAppointments() {
    fetch("http://localhost:5066/api/appointment/all")
    .then(res => res.json())
    .then(data => {
        let html = "";

        data.forEach(a => {
            html += `
                <tr>
                    <td>${a.patientName}</td>
                    <td>${a.doctorName}</td>
                    <td>${new Date(a.date).toLocaleDateString()}</td>
                    <td>${a.timeSlot}</td>
                    <td>${a.status}</td>
                </tr>
            `;
        });

        document.getElementById("appointmentTable").innerHTML = html;
    });
}

function removeDoctor(id) {
    fetch(`http://localhost:5066/api/doctor/remove/${id}`, {
        method: "PUT"
    })
    .then(() => {
        alert("Doctor removed ❌");
        loadDoctors(); // refresh table
    });
}
function doctorRegister() {
    fetch("http://localhost:5066/api/doctor/register", {
        method: "POST",
        headers: {"Content-Type": "application/json"},
        body: JSON.stringify({
            name: document.getElementById("name").value,
            email: document.getElementById("email").value,
            password: document.getElementById("password").value,
            specialization: document.getElementById("specialization").value,
            experience: parseInt(document.getElementById("experience").value),
            fees: parseFloat(document.getElementById("fees").value),
            imageUrl: document.getElementById("imageUrl").value
        })
    })
    .then(() => {
        alert("Registered! Wait for admin approval");
        window.location.href = "login.html";
    });
}

function addDoctor() {
    fetch("http://localhost:5066/api/doctor/add", {
        method: "POST",
        headers: {"Content-Type": "application/json"},
        body: JSON.stringify({
            name: document.getElementById("dname").value,
            specialization: document.getElementById("dspec").value,
            experience: parseInt(document.getElementById("dexp").value),
            fees: parseFloat(document.getElementById("dfees").value),
            email: document.getElementById("demail").value,
            password: document.getElementById("dpass").value,
            imageUrl: document.getElementById("dimg").value
        })
    })
    .then(() => {
        alert("Doctor added successfully ✅");
        loadDoctors(); // refresh table
    });
}
function loadNotifications() {
    const userId = localStorage.getItem("userId");

    fetch(`http://localhost:5066/api/notification/${userId}`)
    .then(res => res.json())
    .then(data => {
        let html = "";

        data.forEach(n => {

            let typeClass = "info";

            if (n.message.includes("ACCEPTED")) typeClass = "success";
            else if (n.message.includes("REJECTED")) typeClass = "error";
            else if (n.message.includes("CANCELLED")) typeClass = "cancel";

            html += `
                <div class="notif-card ${typeClass}">
                    <span>${n.message}</span>
                    <button onclick="markRead(${n.id})">✔</button>
                </div>
            `;
        });

        document.getElementById("notifications").innerHTML = html;
    });
}

function markRead(id) {
    fetch(`http://localhost:5066/api/notification/mark-read/${id}`, {
        method: "PUT"
    })
    .then(() => loadNotifications());
}

let selectedDoctor = 0;
let selectedAppointment = 0;
let selectedRating = 5;

function openReview(doctorId, appointmentId) {

    selectedDoctorId = doctorId;        // ✅ GLOBAL
    selectedAppointmentId = appointmentId;

    document.getElementById("reviewModal").style.display = "block";
}

function closeReview() {
    document.getElementById("reviewModal").style.display = "none";
}

let rating = 0;

function setRating(val) {
    rating = val;
    console.log("Selected rating:", rating);

    // Optional UI highlight
    const stars = document.querySelectorAll(".stars span");
    stars.forEach((s, i) => {
        s.style.opacity = i < val ? "1" : "0.3";
    });
}

function submitReview() {

    console.log("Submit clicked"); // 🔥 DEBUG

    const comment = document.getElementById("comment").value;

    if (rating === 0) {
        alert("Please select rating ⭐");
        return;
    }

    fetch("http://localhost:5066/api/review/add", {
        method: "POST",
        headers: {"Content-Type": "application/json"},
        body: JSON.stringify({
            doctorId: selectedDoctorId,
            appointmentId: selectedAppointmentId,
            patientId: parseInt(localStorage.getItem("userId")),
            rating: rating,
            comment: comment
        })
    })
    .then(res => {
        console.log("Response status:", res.status); // 🔥 DEBUG

        if (!res.ok) {
            return res.text().then(msg => { throw new Error(msg); });
        }

        return res.text();
    })
    .then(() => {
        alert("Review submitted ✅");

        closeReview();

        viewAppointments();
        viewDoctors();
    })
    .catch(err => {
        console.error("Review error:", err); // 🔥 DEBUG
        alert(err.message);
    });
}

function complete(id) {
    fetch(`http://localhost:5066/api/appointment/complete/${id}`, {
        method: "PUT"
    })
    .then(() => {
        alert("Appointment Completed ✅");
        viewDoctorAppointments(); // refresh UI
    });
}

function viewAppointments() {

    const id = localStorage.getItem("userId");

    console.log("Fetching for patient:", id);

    fetch(`http://localhost:5066/api/appointment/patient/${id}`)
    .then(res => res.json())
    .then(data => {

        console.log("Appointments received:", data);

        let html = "";

        if (data.length === 0) {
            html = `<tr><td colspan="5">No Appointments Found</td></tr>`;
        }

        data.forEach(a => {

            let action = "-";

            // 🎯 ACTION LOGIC
            if (a.status === "BOOKED") {
                action = `<button onclick="cancel(${a.appointmentId})">Cancel</button>`;
            }
            else if (a.status === "COMPLETED") {
                action = `<button onclick="openReview(${a.doctorId}, ${a.appointmentId})">⭐ Rate</button>`;
            }

            html += `
                <tr>
                    <td>${new Date(a.date).toLocaleDateString()}</td>
                    <td>${a.timeSlot}</td>
                    <td>${a.doctorName}</td>
                    <td>${a.status}</td>
                    <td>${action}</td>
                </tr>
            `;
        });

        document.getElementById("appointmentsBody").innerHTML = html;
    })
    .catch(err => {
        console.error("Error loading appointments:", err);
    });
}

function goToReviews() {
    window.location.href = "doctor-reviews.html";
}
function viewReviews(doctorId) {
    window.location.href = `doctor-reviews.html?doctorId=${doctorId}`;
}

function loadReviewsPage() {

    let doctorId = localStorage.getItem("userId");

    const params = new URLSearchParams(window.location.search);
    if (params.get("doctorId")) {
        doctorId = params.get("doctorId");
    }

    fetch(`http://localhost:5066/api/review/doctor/${doctorId}`)
    .then(res => res.json())
    .then(data => {

        let html = "";

        if (data.length === 0) {
            html = "<p>No reviews yet</p>";
        }

        // 🔥 👉 PASTE YOUR CODE HERE
        data.forEach(r => {

            const date = new Date(r.createdAt).toLocaleDateString();

            html += `
                <div class="review-card">
                    <div class="review-header">
                        <span class="patient-name">👤 ${r.patientName}</span>
                        <span class="review-date">📅 ${date}</span>
                    </div>

                    <div class="stars">${"⭐".repeat(r.rating)}</div>

                    <p class="comment">${r.comment}</p>
                </div>
            `;
        });

        document.getElementById("reviewContainer").innerHTML = html;
    });
}

function goBack() {
    window.history.back();
}