Go to folder: kafkaworld\dockercompose
Run command: docker-compose up -d
Verification of containers: docker-compose ps
Check topics: docker exec dockercompose_kafka_1 /opt/kafka_2.12-2.1.0/bin/kafka-topics.sh --list --zookeeper zookeeper:2181
Check messages: docker exec kafka-container_kafka_1 /opt/kafka_2.12-2.1.0/bin/kafka-console-consumer.sh --bootstrap-server localhost:9092 --topic Generated --from-beginning
