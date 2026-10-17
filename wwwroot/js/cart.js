// =========================================================================
// CART AJAX HANDLER (Xử lý thêm vào giỏ và cập nhật giỏ hàng theo thời gian thực)
// =========================================================================

let cartDismissTimer = null;

function showCartAlert(message, isSuccess = true) {
    const messageBox = document.getElementById("cart-message");
    if (!messageBox) return;

    if (cartDismissTimer) {
        clearTimeout(cartDismissTimer);
    }

    messageBox.className = isSuccess 
        ? "alert alert-success alert-dismissible fade show shadow-sm mb-4 d-flex align-items-center justify-content-between"
        : "alert alert-danger alert-dismissible fade show shadow-sm mb-4 d-flex align-items-center justify-content-between";

    if (isSuccess) {
        messageBox.innerHTML = `
            <div class="d-flex align-items-center">
                <i class="fa-solid fa-circle-check fs-5 me-2 text-success"></i>
                <span class="fw-semibold">${message}</span>
            </div>
            <div class="d-flex align-items-center gap-2">
                <a href="/Cart" class="btn btn-sm btn-success rounded-pill px-3 fw-bold">
                    <i class="fa-solid fa-cart-shopping me-1"></i> Xem Giỏ Hàng
                </a>
                <button type="button" class="btn-close ms-2" onclick="document.getElementById('cart-message').classList.add('d-none')"></button>
            </div>
        `;
    } else {
        messageBox.innerHTML = `
            <div class="d-flex align-items-center">
                <i class="fa-solid fa-circle-exclamation fs-5 me-2 text-danger"></i>
                <span class="fw-semibold">${message}</span>
            </div>
            <button type="button" class="btn-close" onclick="document.getElementById('cart-message').classList.add('d-none')"></button>
        `;
    }

    messageBox.classList.remove("d-none");

    // Tự động ẩn sau 4 giây
    cartDismissTimer = setTimeout(() => {
        if (messageBox) {
            messageBox.classList.add("d-none");
        }
    }, 4000);
}

function updateCartBadge(count) {
    const badge = document.getElementById("cart-count");
    if (badge) {
        badge.textContent = count;
        badge.style.transition = "transform 0.2s ease-in-out";
        badge.style.transform = "scale(1.35)";
        setTimeout(() => {
            badge.style.transform = "scale(1)";
        }, 250);
    }
}

// Xử lý nút Thêm vào giỏ hàng
document.querySelectorAll(".js-add-to-cart").forEach(form => {
    form.addEventListener("submit", async event => {
        event.preventDefault();

        const button = form.querySelector('button[type="submit"]');
        if (button) button.disabled = true;

        try {
            const response = await fetch(form.action, {
                method: "POST",
                headers: {
                    "X-Requested-With": "XMLHttpRequest",
                    "Accept": "application/json"
                },
                body: new FormData(form)
            });

            // Nếu server trả về chuyển hướng HTML (ví dụ đăng nhập hoặc lỗi chuyển trang)
            const contentType = response.headers.get("content-type");
            if (!contentType || !contentType.includes("application/json")) {
                window.location.href = response.url || "/Cart";
                return;
            }

            const result = await response.json();

            if (!response.ok || !result.success) {
                throw new Error(result.message || "Không thể thêm sản phẩm.");
            }

            updateCartBadge(result.count);
            showCartAlert(result.message || "Đã thêm sản phẩm vào giỏ hàng thành công!", true);
        }
        catch (error) {
            showCartAlert(error.message || "Có lỗi xảy ra khi thêm vào giỏ hàng.", false);
        }
        finally {
            if (button) button.disabled = false;
        }
    });
});

// Xếp hàng các thao tác để tránh gửi giảm/xóa đồng thời.
let cartChangeQueue = Promise.resolve();

document.querySelectorAll(".js-cart-change").forEach(form => {
    form.addEventListener("submit", event => {
        event.preventDefault();

        const button = form.querySelector('button[type="submit"]');
        const formData = new FormData(form);

        if (button) button.disabled = true;

        cartChangeQueue = cartChangeQueue.then(async () => {
            try {
                const response = await fetch(form.action, {
                    method: "POST",
                    headers: {
                        "X-Requested-With": "XMLHttpRequest",
                        "Accept": "application/json"
                    },
                    body: formData
                });

                const contentType = response.headers.get("content-type");
                if (!contentType || !contentType.includes("application/json")) {
                    window.location.reload();
                    return;
                }

                const result = await response.json();

                if (!response.ok || !result.success) {
                    throw new Error(result.message || "Không thể cập nhật giỏ hàng.");
                }

                // Cập nhật badge
                updateCartBadge(result.count);

                // Đối chiếu các dòng đang hiển thị với giỏ mới
                document.querySelectorAll(
                    "#cart-table tbody tr[data-product-id]"
                ).forEach(row => {
                    const productId = Number(row.dataset.productId);

                    const item = result.items.find(
                        item => item.productId === productId
                    );

                    if (!item) {
                        row.remove();
                        return;
                    }

                    const qtyElem = row.querySelector(".cart-quantity");
                    if (qtyElem) qtyElem.textContent = item.quantity;

                    const priceElem = row.querySelector(".cart-line-total");
                    if (priceElem) priceElem.textContent = formatCartMoney(item.totalPrice);
                });

                // Cập nhật tổng tiền
                const totalElem = document.getElementById("cart-total");
                if (totalElem) {
                    totalElem.textContent = formatCartMoney(result.total);
                }

                // Điều chỉnh giao diện khi giỏ trống
                const isEmpty = !result.items || result.items.length === 0;

                const cartTable = document.getElementById("cart-table");
                if (cartTable) cartTable.classList.toggle("d-none", isEmpty);

                const cartSummary = document.getElementById("cart-summary");
                if (cartSummary) cartSummary.classList.toggle("d-none", isEmpty);

                const cartEmpty = document.getElementById("cart-empty");
                if (cartEmpty) cartEmpty.classList.toggle("d-none", !isEmpty);

                showCartAlert(result.message, true);
            }
            catch (error) {
                showCartAlert(error.message, false);
            }
            finally {
                if (button) button.disabled = false;
            }
        });
    });
});

function formatCartMoney(value) {
    return new Intl.NumberFormat("vi-VN", {
        maximumFractionDigits: 0
    }).format(value) + " đ";
}