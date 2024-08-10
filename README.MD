# CSharpNetApp

Домашние задания к курсу "Разработка сетевого приложения на C#" на GeekBrains.

## Урок 5. Базы данных: Entity framework, code first/db first

### Задание

Реализуйте тип сообщений List, при котором клиент будет получать все непрочитанные сообщения с сервера.

### Дополнительное задание:

Сделать связь с базой данных на основе подхода Code First с использованием Microsoft Entity Framework. Разработать для этого классы Message и User, контекст базы данных, выполнить миграцию и обновление базы.

### Решение

Создано новое решение в Visual Studio. Добавлен новый проект консольного приложения ChatDB, содержащий классы Message, User, ChatDBContext.

Выполнена миграция и обновление базы данных. Подключение выполнено к развернутой базе PostgreSQL.

![Результат обновления базы](https://s935sas.storage.yandex.net/rdisk/92b07e75beb745da851b4ec794abf813d2f42ed60107bf7491e1d4aa255f064e/66b79e3c/MmrzcZdFqBsuAaoM02nEu2axOIDlYIKu7zDfcogN-_w6VXt0FbxHtjs4ZGjU6cwSnHX2MRdEP_yaTUWlZhLfpQ==?uid=0&filename=2024-08-10_16-04-55.png&disposition=attachment&hash=7rkDiOeP48/oc6MyanuP9B6lkE%2B1rGZZjkEbVWQg1D6ir9ZvDH0ls03/p/im81dHq/J6bpmRyOJonT3VoXnDag%3D%3D&limit=0&content_type=image%2Fpng&owner_uid=360618779&fsize=43541&hid=ddd5e9f37a52115ba95c1b63164db642&media_type=image&tknv=v2&ts=61f574a370700&s=af11341189652a95d44434b93a0e7f3944c18a4f0135c637c0cfa4a0016b7a85&pb=U2FsdGVkX1-mbLJ0p6z5nphMEonEOx-1z-gV7OO3HgAWZQJQ2NFocDGaYFO8uK27ZXDO8dhFthVkGIiIBTUqYWCfbXmYn-nkH3OnpRmo3yA)
