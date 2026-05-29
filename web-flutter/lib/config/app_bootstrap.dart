import 'package:flutter/material.dart';
import 'package:flutter_localizations/flutter_localizations.dart';

import 'app_dependencies.dart';
import '../routing/app_router.dart';
import '../ui/core/l10n/app_localization.dart';
import '../ui/core/theme/app_theme.dart';

class AppBootstrap extends StatelessWidget {
  const AppBootstrap({super.key});

  @override
  Widget build(BuildContext context) {
    const dependencies = AppDependencies();

    return MaterialApp(
      title: 'BabaPlay',
      debugShowCheckedModeBanner: false,
      theme: AppTheme.light(),
      locale: const Locale('pt', 'BR'),
      localizationsDelegates: const [
        AppLocalization.delegate,
        GlobalMaterialLocalizations.delegate,
        GlobalWidgetsLocalizations.delegate,
        GlobalCupertinoLocalizations.delegate,
      ],
      supportedLocales: const [
        Locale('pt', 'BR'),
      ],
      initialRoute: RoutePaths.login,
      onGenerateRoute: (settings) => AppRouter.onGenerateRoute(
        settings,
        dependencies,
      ),
    );
  }
}
