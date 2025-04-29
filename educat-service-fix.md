# Исправление ошибки сервиса educat

Ошибка `status=200/CHDIR` означает, что systemd не может получить доступ к рабочей директории.

## Проверьте наличие директории

```bash
# Убедитесь, что директория существует
ls -la /var/www/educat/publish
```

## Исправьте права доступа

```bash
# Установите корректные права доступа для директории и файлов
sudo chown -R www-data:www-data /var/www/educat
sudo chmod -R 755 /var/www/educat
```

## Проверьте, что файлы опубликованы в нужное место

Если директория `/var/www/educat/publish` не существует:

```bash
# Создайте директорию, если ее не существует
sudo mkdir -p /var/www/educat/publish

# Перепубликуйте проект
cd /var/www/educat
sudo dotnet publish src/Vibetech.Educat/Vibetech.Educat.csproj -c Release -o ./publish
```

## Исправьте конфигурацию сервиса

```bash
sudo nano /etc/systemd/system/educat.service
```

Убедитесь, что путь к файлам в systemd файле корректный:

```
[Unit]
Description=Vibetech Educat Web App

[Service]
WorkingDirectory=/var/www/educat/publish
ExecStart=/usr/bin/dotnet /var/www/educat/publish/Vibetech.Educat.dll
Restart=always
RestartSec=10
SyslogIdentifier=vibetech-educat
User=www-data
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=DOTNET_PRINT_TELEMETRY_MESSAGE=false

[Install]
WantedBy=multi-user.target
```

После изменения конфигурации:

```bash
sudo systemctl daemon-reload
sudo systemctl restart educat.service
sudo systemctl status educat.service
```

## Проверка логов

Для детальной информации об ошибках посмотрите логи:

```bash
sudo journalctl -u educat.service --since today
``` 