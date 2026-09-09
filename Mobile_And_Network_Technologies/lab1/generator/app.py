import json
import os
import random
import time
from datetime import datetime, timezone

from paho.mqtt.client import Client
from paho.mqtt.enums import CallbackAPIVersion

host = os.getenv("MQTT_HOST", "mosquitto")
port = int(os.getenv("MQTT_PORT", "1883"))
device_id = os.getenv("DEVICE_ID", "student-15")
interval = float(os.getenv("INTERVAL", "5"))
topic = f"iot/{device_id}/telemetry"

client = Client(
    callback_api_version=CallbackAPIVersion.VERSION2,
    client_id=f"{device_id}-simulator",
)

while True:
    try:
        client.connect(host, port, 60)
        break
    except OSError:
        print("Broker недоступен, повтор через 2 секунды...")

time.sleep(2)
client.loop_start()

while True:
    message = {
        "deviceId": device_id,
        "value": round(random.uniform(20.0, 30.0), 1),
        "timestamp": datetime.now(timezone.utc).isoformat(),
    }

    payload = json.dumps(message)
    client.publish(topic, payload)
    print(f"{topic}: {payload}")
    time.sleep(interval)
