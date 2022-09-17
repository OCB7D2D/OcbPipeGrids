use strict;
use warnings;
package Cube;
use base qw(Mesh);
use Math::Vector::Real;
use Quad;

sub new
{
	my $package = shift;
	my $self = $package->SUPER::new;
	# Other subconstructor stuff here

	my ($size) = @_;

	my $x = $size / 2;
	my $y = $size / 2;
	my $z = $size / 2;

	$self->AddFace(Quad->new(
		Vertex->new(V(+$x, +$y, -$z), V(+1,0,0)),
		Vertex->new(V(+$x, +$y, +$z), V(+1,0,0)),
		Vertex->new(V(+$x, -$y, +$z), V(+1,0,0)),
		Vertex->new(V(+$x, -$y, -$z), V(+1,0,0)),
	));
	$self->AddFace(Quad->new(
		Vertex->new(V(-$x, +$y, -$z), V(-1,0,0)),
		Vertex->new(V(-$x, -$y, -$z), V(-1,0,0)),
		Vertex->new(V(-$x, -$y, +$z), V(-1,0,0)),
		Vertex->new(V(-$x, +$y, +$z), V(-1,0,0)),
	));
	$self->AddFace(Quad->new(
		Vertex->new(V(+$x, +$y, -$z), V(0,+1,0)),
		Vertex->new(V(-$x, +$y, -$z), V(0,+1,0)),
		Vertex->new(V(-$x, +$y, +$z), V(0,+1,0)),
		Vertex->new(V(+$x, +$y, +$z), V(0,+1,0)),
	));
	$self->AddFace(Quad->new(
		Vertex->new(V(+$x, -$y, -$z), V(0,-1,0)),
		Vertex->new(V(+$x, -$y, +$z), V(0,-1,0)),
		Vertex->new(V(-$x, -$y, +$z), V(0,-1,0)),
		Vertex->new(V(-$x, -$y, -$z), V(0,-1,0)),
	));
	$self->AddFace(Quad->new(
		Vertex->new(V(+$x, -$y, +$z), V(0,0,+1)),
		Vertex->new(V(+$x, +$y, +$z), V(0,0,+1)),
		Vertex->new(V(-$x, +$y, +$z), V(0,0,+1)),
		Vertex->new(V(-$x, -$y, +$z), V(0,0,+1)),
	));
	$self->AddFace(Quad->new(
		Vertex->new(V(+$x, -$y, -$z), V(0,0,-1)),
		Vertex->new(V(-$x, -$y, -$z), V(0,0,-1)),
		Vertex->new(V(-$x, +$y, -$z), V(0,0,-1)),
		Vertex->new(V(+$x, +$y, -$z), V(0,0,-1)),
	));

	return $self;
}

1;