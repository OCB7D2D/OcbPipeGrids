use strict;
use warnings;
package Triangle;
use base qw(Face);
use Math::Vector::Real;
use Line;

sub VertexCount { 3 }

sub Lines
{
	return $_[0]->{lines} if exists $_[0]->{lines};
	return $_[0]->{lines} = (
		Line->new($_[0]->Vertice(0), $_[0]->Vertice(1)),
		Line->new($_[0]->Vertice(1), $_[0]->Vertice(2)),
		Line->new($_[0]->Vertice(2), $_[0]->Vertice(0)),
	);
}

sub new
{
	my $package = shift;
	my $self = $package->SUPER::new(@_);
	# Other subconstructor stuff here
	return $self;
}

sub CalcFaceNormal
{
	my $self = shift;
	my $U = $self->Vertice(1)->Position - $self->Vertice(0)->Position;
	my $V1 = $self->Vertice(2)->Position - $self->Vertice(0)->Position;
	return V(
		$U->[1] * $V1->[2] - $U->[2] * $V1->[1],
		$U->[2] * $V1->[0] - $U->[0] * $V1->[2],
		$U->[0] * $V1->[1] - $U->[1] * $V1->[0],
	)->versor;
}

sub Compress
{
	# nothing to do
}

sub SetUVs
{
	my ($self, $uv1, $uv2, $uv3) = @_;
	$self->{uvs} = [$uv1, $uv2, $uv3];
	return $self;
}

1;