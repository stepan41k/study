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

telemetry_topic = f"iot/{device_id}/telemetry"
command_topic   = f"iot/{device_id}/commands"
state_topic     = f"iot/{device_id}/state"
status_topic    = f"iot/{device_id}/status"

fan_state = "OFF"
heater_state = "OFF"

client = Client(
    callback_api_version=CallbackAPIVersion.VERSION2,
    client_id=f"{device_id}-simulator",
)

def now():
    return datetime.now(timezone.utc).isoformat()


def publish_state(client):
    message = {
        "deviceId": device_id,
        "fan": fan_state,
        "heater": heater_state, 
        "timestamp": now(),
        }
    client.publish(state_topic, json.dumps(message), qos=1, retain=True)

def on_connect(client, userdata, flags, reason_code, properties):
    if reason_code.is_failure:
        print(f"Ошибка подключения: {reason_code}")
        return

    print("Подключено к MQTT Broker")
    client.subscribe(command_topic, qos=1)
    client.publish(status_topic, "online", qos=1, retain=True)
    publish_state(client)


def on_message(client, userdata, msg):
    global fan_state, heater_state
    try:
        data = json.loads(msg.payload.decode("utf-8"))
    except Exception:
        print("Получена некорректная команда (не JSON)")
        return

    command = data.get("command")
    val = data.get("value")

    # Обработка вентилятора
    if command == "setFan":
        if val is True:
            fan_state = "ON"
        elif val is False:
            fan_state = "OFF"
        else:
            print("Некорректное значение для setFan")
            return
        print(f"Команда выполнена. fan = {fan_state}")
        publish_state(client)

    elif command == "setHeater":
        if val is True:
            heater_state = "ON"
        elif val is False:
            heater_state = "OFF"
        else:
            print("Некорректное значение команды")
            return
        print(f"Команда выполнена. heater = {heater_state}")
        publish_state(client)

while True:
    try:
        client.will_set(
            status_topic,
            payload="offline",
            qos=1,
            retain=True
        )

        client.connect(host, port, 60)

        break
    except OSError:
        print("Broker недоступен, повтор через 2 секунды...")

time.sleep(2)
client.loop_start()

client.on_connect = on_connect
client.on_message = on_message

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
