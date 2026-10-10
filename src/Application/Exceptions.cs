namespace Application;

public sealed class InvalidInputException(string message) : Exception(message);
public sealed class UserNotFoundException() : Exception("El usuario no existe.");
public sealed class EmailConflictException() : Exception("El email ya pertenece a otro usuario.");
public sealed class InvalidCredentialsException() : Exception("Credenciales incorrectas.");
