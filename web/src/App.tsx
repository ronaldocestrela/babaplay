import { RouterProvider } from '@tanstack/react-router'
import { AppProviders } from './core/providers/AppProviders'
import { router } from './router'

export default function App() {
  return (
    <AppProviders>
      <RouterProvider router={router} />
    </AppProviders>
  )
}
