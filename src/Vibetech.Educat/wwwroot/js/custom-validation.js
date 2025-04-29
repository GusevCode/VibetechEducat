// Кастомные сообщения об ошибках валидации на русском языке
$(function () {
    // Переопределение сообщений об ошибках jQuery Validate
    $.extend($.validator.messages, {
        required: "Это поле обязательно для заполнения",
        remote: "Пожалуйста, исправьте это поле",
        email: "Пожалуйста, введите корректный адрес электронной почты",
        url: "Пожалуйста, введите корректный URL",
        date: "Пожалуйста, введите корректную дату",
        dateISO: "Пожалуйста, введите корректную дату в формате ISO",
        number: "Пожалуйста, введите число",
        digits: "Пожалуйста, введите только цифры",
        creditcard: "Пожалуйста, введите правильный номер кредитной карты",
        equalTo: "Пожалуйста, введите такое же значение еще раз",
        extension: "Пожалуйста, выберите файл с правильным расширением",
        maxlength: $.validator.format("Пожалуйста, введите не более {0} символов"),
        minlength: $.validator.format("Пожалуйста, введите не менее {0} символов"),
        rangelength: $.validator.format("Пожалуйста, введите значение длиной от {0} до {1} символов"),
        range: $.validator.format("Пожалуйста, введите число от {0} до {1}"),
        max: $.validator.format("Пожалуйста, введите число, меньшее или равное {0}"),
        min: $.validator.format("Пожалуйста, введите число, большее или равное {0}")
    });

    // Добавление кастомной валидации для русских ФИО
    $.validator.addMethod(
        "cyrillic",
        function(value, element) {
            return this.optional(element) || /^[а-яА-ЯёЁ\s-]+$/i.test(value);
        },
        "Пожалуйста, используйте только кириллические символы"
    );
    
    // Переопределение сообщений для валидации даты
    $.validator.addMethod(
        "date",
        function(value, element) {
            if (this.optional(element)) {
                return true;
            }
            // Проверка на валидность даты
            var result = false;
            
            // Try parsing Russian date format (dd.mm.yyyy)
            if (/^\d{1,2}\.\d{1,2}\.\d{4}$/.test(value)) {
                var parts = value.split('.');
                var day = parseInt(parts[0], 10);
                var month = parseInt(parts[1], 10) - 1;
                var year = parseInt(parts[2], 10);
                var date = new Date(year, month, day);
                result = !isNaN(date.getTime()) && 
                         date.getDate() === day && 
                         date.getMonth() === month && 
                         date.getFullYear() === year;
                return result;
            }
            
            // Try standard date parsing as fallback
            try {
                var date = new Date(value);
                result = !isNaN(date.getTime());
            } catch (e) {
                result = false;
            }
            return result;
        },
        "Пожалуйста, введите корректную дату"
    );
    
    // Переопределение дефолтного валидатора для конвертации строк в DateTime
    $.validator.methods.number = function(value, element) {
        return this.optional(element) || !isNaN(value);
    };

    // Улучшение отображения ошибок валидации
    $.validator.setDefaults({
        errorClass: 'text-danger',
        highlight: function(element) {
            $(element).addClass('is-invalid');
        },
        unhighlight: function(element) {
            $(element).removeClass('is-invalid');
        },
        errorPlacement: function(error, element) {
            if (element.parent('.input-group').length) {
                error.insertAfter(element.parent());
            } else if (element.prop('type') === 'radio' || element.prop('type') === 'checkbox') {
                error.appendTo(element.closest('.form-group'));
            } else {
                error.insertAfter(element);
            }
        }
    });
}); 