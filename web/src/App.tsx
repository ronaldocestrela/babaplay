import { RouterProvider } from '@tanstack/react-router'
import { AppProviders } from './core/providers/AppProviders'
import { ErrorBoundary } from './core/components/ErrorBoundary'
import { router } from './router'

export default function App() {
  return (
    <ErrorBoundary>
      <AppProviders>
        <RouterProvider router={router} />
      </AppProviders>
    </ErrorBoundary>
  )
}
