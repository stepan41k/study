# def process_order(price, quantity, client_type):
#     result = price * quantity
    
#     if client_type == "vip":
#         if result > 10000:
#             result = result - result * 0.2
#         else:
#             result = result - result * 0.1
            
#     if result > 50000:
#         result = result - 1000

#     print(result)
#     return result

VIP_CLIENT_TYPE = "vip"
VIP_HIGH_THRESHOLD = 10_000
VIP_HIGH_DISCOUNT = 0.20
VIP_LOW_DISCOUNT = 0.10

BULK_DISCOUNT_THRESHOLD = 50_000
BULK_DISCOUNT_AMOUNT = 1_000


def process_order(price: float, quantity: int, client_type: str = "standard") -> float:
    if price < 0 or quantity < 0:
        raise ValueError("Цена и количество не могут быть отрицательными")

    total = price * quantity

    if client_type and client_type.strip().lower() == VIP_CLIENT_TYPE:
        discount_rate = VIP_HIGH_DISCOUNT if total > VIP_HIGH_THRESHOLD else VIP_LOW_DISCOUNT
        total *= (1 - discount_rate)

    if total > BULK_DISCOUNT_THRESHOLD:
        total -= BULK_DISCOUNT_AMOUNT

    return round(total, 2)
