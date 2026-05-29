import 'package:flutter/material.dart';

import '../config/app_dependencies.dart';
import '../ui/auth/pages/login_page.dart';

class RoutePaths {
  static const login = '/login';
  static const dashboard = '/';
}

class AppRouter {
  static Route<dynamic> onGenerateRoute(
    RouteSettings settings,
    AppDependencies dependencies,
  ) {
    switch (settings.name) {
      case RoutePaths.login:
      default:
        return MaterialPageRoute<void>(
          builder: (_) => LoginPage(
            viewModel: dependencies.createLoginViewModel(),
          ),
          settings: settings,
        );
    }
  }
}
