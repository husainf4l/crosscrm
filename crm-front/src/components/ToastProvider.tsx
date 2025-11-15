'use client';

import { useToast } from '@/hooks/useToast';
import { ToastContainer } from '@/components/ui';

export default function ToastProvider({ children }: { children: React.ReactNode }) {
  const { toasts, removeToast } = useToast();

  return (
    <>
      {children}
      <ToastContainer toasts={toasts} onRemove={removeToast} />
    </>
  );
}

