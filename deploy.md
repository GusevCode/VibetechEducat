# Инструкция по деплою проекта Vibetech.Educat

## 1. Сборка и публикация проекта локально

1. Откройте PowerShell и перейдите в директорию с проектом:
```powershell
cd C:\repos\EducatGithub\VibetechEducat
```

2. Опубликуйте проект в папку 'publish':
```powershell
dotnet publish src/Vibetech.Educat/Vibetech.Educat.csproj -c Release -o ./publish
```

## 2. Отправка в репозиторий GitHub

1. Если репозиторий еще не настроен, выполните:
```powershell
git init
git add .
git commit -m "Initial commit"
git branch -M main
git remote add origin ВАШ_GITHUB_РЕПОЗИТОРИЙ_URL
git push -u origin main
```

2. Если репозиторий уже настроен:
```powershell
git add .
git commit -m "Update for deployment"
git push
```

## 3. Настройка на сервере

1. Подключитесь к серверу через SSH

2. Установите необходимые компоненты (если еще не установлены):
```bash
sudo apt-get update
sudo apt-get install -y apt-transport-https aspnetcore-runtime-8.0
```

3. Клонируйте репозиторий:
```bash
git clone ВАШ_GITHUB_РЕПОЗИТОРИЙ_URL /var/www/educat
```

4. Создайте сервис systemd для запуска приложения:
```bash
sudo nano /etc/systemd/system/educat.service
```

5. Содержимое файла educat.service:
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

6. Активируйте и запустите сервис:
```bash
sudo systemctl enable educat.service
sudo systemctl start educat.service
```

7. Проверьте статус:
```bash
sudo systemctl status educat.service
```

## 4. Обновление на сервере

1. Перейдите в директорию проекта:
```bash
cd /var/www/educat
```

2. Получите последние изменения:
```bash
git pull
```

3. Перестройте и опубликуйте:
```bash
dotnet publish src/Vibetech.Educat/Vibetech.Educat.csproj -c Release -o ./publish
```

4. Перезапустите сервис:
```bash
sudo systemctl restart educat.service
```

## Примечания
- База данных должна быть уже настроена на сервере с теми же параметрами подключения.
- Nginx уже должен быть настроен для проксирования на localhost:5091.
- Если нужны изменения в appsettings.Production.json, внесите их перед сборкой. 