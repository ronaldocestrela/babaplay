import '../data/auth/repositories/auth_repository_impl.dart';
import '../domain/auth/usecases/login_usecase.dart';
import '../ui/auth/viewmodels/login_viewmodel.dart';

class AppDependencies {
  const AppDependencies();

  LoginViewModel createLoginViewModel() {
    return LoginViewModel(
      LoginUseCase(
        AuthRepositoryImpl(),
      ),
    );
  }
}
