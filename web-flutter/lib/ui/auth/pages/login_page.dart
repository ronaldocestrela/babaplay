import 'package:flutter/material.dart';

import '../../core/l10n/app_localization.dart';
import '../viewmodels/login_viewmodel.dart';

class LoginPage extends StatefulWidget {
  const LoginPage({
    super.key,
    required this.viewModel,
  });

  final LoginViewModel viewModel;

  @override
  State<LoginPage> createState() => _LoginPageState();
}

class _LoginPageState extends State<LoginPage> {
  final _emailController = TextEditingController();
  final _passwordController = TextEditingController();

  @override
  void dispose() {
    _emailController.dispose();
    _passwordController.dispose();
    super.dispose();
  }

  Future<void> _submit() async {
    final result = await widget.viewModel.login(
      _emailController.text,
      _passwordController.text,
    );

    if (!mounted) {
      return;
    }

    result.fold(
      (_) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text(context.tr('loginSuccess'))),
        );
      },
      (exception) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text(exception.message)),
        );
      },
    );

    setState(() {});
  }

  @override
  Widget build(BuildContext context) {
    final state = widget.viewModel.state;

    return Scaffold(
      body: Center(
        child: ConstrainedBox(
          constraints: const BoxConstraints(maxWidth: 440),
          child: Padding(
            padding: const EdgeInsets.all(16),
            child: Card(
              child: Padding(
                padding: const EdgeInsets.all(16),
                child: Column(
                  mainAxisSize: MainAxisSize.min,
                  crossAxisAlignment: CrossAxisAlignment.stretch,
                  children: [
                    Text(
                      context.tr('appName'),
                      style: Theme.of(context).textTheme.headlineSmall,
                    ),
                    const SizedBox(height: 12),
                    TextField(
                      controller: _emailController,
                      decoration: InputDecoration(labelText: context.tr('email')),
                    ),
                    const SizedBox(height: 8),
                    TextField(
                      controller: _passwordController,
                      obscureText: true,
                      decoration: InputDecoration(labelText: context.tr('password')),
                    ),
                    const SizedBox(height: 16),
                    FilledButton(
                      onPressed: state is LoginLoading ? null : _submit,
                      child: switch (state) {
                        LoginLoading() => const SizedBox(
                            width: 16,
                            height: 16,
                            child: CircularProgressIndicator(strokeWidth: 2),
                          ),
                        _ => Text(context.tr('signIn')),
                      },
                    ),
                    const SizedBox(height: 8),
                    switch (state) {
                      LoginFailure(:final errorMessage) => Text(
                          errorMessage,
                          style: const TextStyle(color: Color(0xFFBA1A1A)),
                        ),
                      LoginSuccess() => Text(context.tr('sessionActive')),
                      LoginIdle() || LoginLoading() => const SizedBox.shrink(),
                    },
                  ],
                ),
              ),
            ),
          ),
        ),
      ),
    );
  }
}
