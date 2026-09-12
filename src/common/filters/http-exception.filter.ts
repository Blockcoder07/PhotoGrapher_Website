import { ArgumentsHost, Catch, ExceptionFilter, HttpException, HttpStatus } from '@nestjs/common';
import type { Response } from 'express';
import { ApiResponse } from '../dto/api-response.dto.js';

/** Mirrors GlobalExceptionMiddleware.cs: maps thrown errors to the same ApiResponse shape/status codes. */
@Catch()
export class AllExceptionsFilter implements ExceptionFilter {
  catch(exception: unknown, host: ArgumentsHost) {
    const ctx = host.switchToHttp();
    const response = ctx.getResponse<Response>();

    if (exception instanceof HttpException) {
      const status = exception.getStatus();
      const body = exception.getResponse();
      const message = typeof body === 'string' ? body : ((body as any)?.message ?? exception.message);
      const errors = typeof body === 'object' && Array.isArray((body as any)?.message)
        ? (body as any).message
        : [];
      response.status(status).json(
        ApiResponse.fail(Array.isArray(message) ? 'Invalid request' : message, errors),
      );
      return;
    }

    if (exception instanceof Error) {
      response
        .status(HttpStatus.INTERNAL_SERVER_ERROR)
        .json(ApiResponse.fail('An error occurred while processing your request.', [exception.message]));
      return;
    }

    response
      .status(HttpStatus.INTERNAL_SERVER_ERROR)
      .json(ApiResponse.fail('An error occurred while processing your request.'));
  }
}
