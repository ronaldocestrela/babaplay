typedef AsyncResult<T> = Future<Result<T>>;

sealed class Result<T> {
  const Result();

  R fold<R>(R Function(T value) onSuccess, R Function(AppException exception) onFailure);

  Result<R> map<R>(R Function(T value) transform) {
    return fold(
      (value) => Success<R>(transform(value)),
      Failure<R>.new,
    );
  }

  AsyncResult<R> flatMap<R>(AsyncResult<R> Function(T value) transform) async {
    return fold(
      transform,
      (exception) async => Failure<R>(exception),
    );
  }
}

final class Success<T> extends Result<T> {
  const Success(this.value);

  final T value;

  @override
  R fold<R>(R Function(T value) onSuccess, R Function(AppException exception) onFailure) {
    return onSuccess(value);
  }
}

final class Failure<T> extends Result<T> {
  const Failure(this.exception);

  final AppException exception;

  @override
  R fold<R>(R Function(T value) onSuccess, R Function(AppException exception) onFailure) {
    return onFailure(exception);
  }
}

class Unit {
  const Unit();
}

const unit = Unit();

class AppException {
  const AppException(this.message);

  final String message;

  factory AppException.business(String message) {
    return AppException(message);
  }

  factory AppException.fatal(String message) {
    return AppException(message);
  }
}
