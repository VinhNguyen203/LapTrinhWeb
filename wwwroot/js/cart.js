document.querySelectorAll(".js-add-to-cart").forEach(form => {
    form.addEventListener("submit", async event => {
        event.preventDefault();

        const button = form.querySelector('button[type="submit"]');
        const messageBox = document.getElementById("cart-message");

        button.disabled = true;

        try {
            const response = await fetch(form.action, {
                method: "POST",
                body: new FormData(form)
            });

            const result = await response.json();

            if (!response.ok || !result.success) {
                throw new Error(result.message || "Không thể thêm sản phẩm.");
            }

            const badge = document.getElementById("cart-count");

            if (badge) {
                badge.textContent = result.count;
            }

            messageBox.textContent = result.message;
            messageBox.className = "alert alert-success";
        }
        catch (error) {
            messageBox.textContent = error.message;
            messageBox.className = "alert alert-danger";
        }
        finally {
            button.disabled = false;
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

        button.disabled = true;

        cartChangeQueue = cartChangeQueue.then(async () => {
            const messageBox = document.getElementById("cart-message");

            try {
                const response = await fetch(form.action, {
                    method: "POST",
                    body: formData
                });

                const result = await response.json();

                if (!response.ok || !result.success) {
                    throw new Error(
                        result.message || "Không thể cập nhật giỏ hàng.");
                }

                // Cập nhật badge.
                const badge = document.getElementById("cart-count");

                if (badge) {
                    badge.textContent = result.count;
                }

                // Đối chiếu các dòng đang hiển thị với giỏ mới.
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

                    row.querySelector(".cart-quantity").textContent =
                        item.quantity;

                    row.querySelector(".cart-line-total").textContent =
                        formatCartMoney(item.totalPrice);
                });

                // Cập nhật tổng tiền.
                document.getElementById("cart-total").textContent =
                    formatCartMoney(result.total);

                // Điều chỉnh giao diện khi giỏ trống.
                const isEmpty = result.items.length === 0;

                document.getElementById("cart-table")
                    .classList.toggle("d-none", isEmpty);

                document.getElementById("cart-summary")
                    .classList.toggle("d-none", isEmpty);

                document.getElementById("cart-empty")
                    .classList.toggle("d-none", !isEmpty);

                messageBox.textContent = result.message;
                messageBox.className = "alert alert-success";
            }
            catch (error) {
                messageBox.textContent = error.message;
                messageBox.className = "alert alert-danger";
            }
            finally {
                button.disabled = false;
            }
        });
    });
});

function formatCartMoney(value) {
    return new Intl.NumberFormat("vi-VN", {
        maximumFractionDigits: 0
    }).format(value) + " đ";
}