"""Initial empty migration

Revision ID: 6e02a1674388
Revises: b633391cccf3
Create Date: 2025-11-15 04:36:15.003598

"""
from typing import Sequence, Union

from alembic import op
import sqlalchemy as sa


# revision identifiers, used by Alembic.
revision: str = '6e02a1674388'
down_revision: Union[str, None] = 'b633391cccf3'
branch_labels: Union[str, Sequence[str], None] = None
depends_on: Union[str, Sequence[str], None] = None


def upgrade() -> None:
    pass


def downgrade() -> None:
    pass

