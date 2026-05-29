import 'dart:convert';

import 'package:flutter/material.dart';
import 'package:flutter/services.dart';

class AppLocalization {
  AppLocalization(this._localizedValues);

  final Map<String, String> _localizedValues;

  static const LocalizationsDelegate<AppLocalization> delegate = _AppLocalizationDelegate();

  static AppLocalization of(BuildContext context) {
    final localization = Localizations.of<AppLocalization>(context, AppLocalization);
    return localization ?? AppLocalization(const {});
  }

  String translate(String key) {
    return _localizedValues[key] ?? key;
  }
}

class _AppLocalizationDelegate extends LocalizationsDelegate<AppLocalization> {
  const _AppLocalizationDelegate();

  @override
  bool isSupported(Locale locale) {
    return locale.languageCode == 'pt';
  }

  @override
  Future<AppLocalization> load(Locale locale) async {
    final raw = await rootBundle.loadString('assets/translations/pt-BR.json');
    final Map<String, dynamic> jsonMap = jsonDecode(raw) as Map<String, dynamic>;
    final values = jsonMap.map(
      (key, value) => MapEntry(key, value.toString()),
    );
    return AppLocalization(values);
  }

  @override
  bool shouldReload(_AppLocalizationDelegate old) {
    return false;
  }
}

extension LocalizationBuildContext on BuildContext {
  String tr(String key) {
    return AppLocalization.of(this).translate(key);
  }
}
