import type { Metadata } from "next";
import "./globals.css";
import { PageTransition } from "@/components/ui";
import ToastProvider from "@/components/ToastProvider";

export const metadata: Metadata = {
  title: "CrossCRM",
  description: "Modern CRM Application - Apple-Style Design",
};

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html lang="en" className="antialiased">
      <body className="min-h-screen bg-background text-foreground">
        <ToastProvider>
          <PageTransition>
            {children}
          </PageTransition>
        </ToastProvider>
      </body>
    </html>
  );
}
