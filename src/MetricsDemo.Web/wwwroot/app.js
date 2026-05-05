const $ = (sel) => document.querySelector(sel);

async function refreshHealth() {
  const pill = $("#healthPill");
  try {
    const res = await fetch("/api/health");
    const data = await res.json();
    pill.textContent = data.status === "ok" ? "API healthy" : "degraded";
    pill.classList.toggle("ok", data.status === "ok");
  } catch {
    pill.textContent = "offline";
    pill.classList.remove("ok");
  }
}

function showOut(id, obj) {
  $(id).textContent = JSON.stringify(obj, null, 2);
}

$("#form-order").addEventListener("submit", async (e) => {
  e.preventDefault();
  const fd = new FormData(e.target);
  const body = {
    customerId: fd.get("customerId"),
    tier: fd.get("tier"),
    subtotal: Number(fd.get("subtotal")),
    itemCount: Number(fd.get("itemCount")),
    isWeekend: fd.get("isWeekend") === "on",
    paymentMethod: fd.get("paymentMethod"),
    couponCode: fd.get("couponCode") || null,
  };
  const res = await fetch("/api/orders/process", {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(body),
  });
  showOut("#out-order", await res.json());
});

$("#form-user").addEventListener("submit", async (e) => {
  e.preventDefault();
  const fd = new FormData(e.target);
  const body = {
    email: fd.get("email"),
    displayName: fd.get("displayName"),
    age: Number(fd.get("age")),
    countryCode: fd.get("countryCode"),
    acceptsMarketing: fd.get("acceptsMarketing") === "on",
    phone: fd.get("phone") || null,
  };
  const res = await fetch("/api/users/validate", {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(body),
  });
  showOut("#out-user", await res.json());
});

$("#form-price").addEventListener("submit", async (e) => {
  e.preventDefault();
  const fd = new FormData(e.target);
  const params = new URLSearchParams({
    sku: fd.get("sku"),
    qty: fd.get("qty"),
    region: fd.get("region"),
  });
  const res = await fetch(`/api/inventory/price?${params}`);
  showOut("#out-price", await res.json());
});

$("#form-ship").addEventListener("submit", async (e) => {
  e.preventDefault();
  const fd = new FormData(e.target);
  const params = new URLSearchParams({
    weightKg: fd.get("weightKg"),
    zone: fd.get("zone"),
    express: fd.get("express") === "on",
  });
  const res = await fetch(`/api/shipping/quote?${params}`);
  showOut("#out-ship", await res.json());
});

refreshHealth();
setInterval(refreshHealth, 15000);
