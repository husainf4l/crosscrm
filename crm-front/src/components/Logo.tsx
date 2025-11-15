import Link from 'next/link';

interface LogoProps {
  className?: string;
}

export default function Logo({ className = '' }: LogoProps) {
  return (
    <Link 
      href="/" 
      className={`flex items-center gap-1.5 transition-opacity hover:opacity-80 ${className}`}
    >
      <div className="flex items-center justify-center">
        <span className="text-xl font-semibold text-blue tracking-tight" style={{ color: 'var(--blue)' }}>
          CROSS
        </span>
        <span className="text-xl font-medium tracking-tight ml-0.5" style={{ color: 'var(--gray-900)' }}>
          CRM
        </span>
      </div>
    </Link>
  );
}