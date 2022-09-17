use strict;
use warnings;
package Path;
use constant PI => 4 * atan2(1, 1);
use base qw(Vertices);
use Math::Vector::Real;
use Quad;

sub CopyTo
{
	my $self = shift;
	my ($mesh, $side) = @_;
	my $clone = $self->clone();
	$mesh->CopyVertices($clone);
}

sub CloseArea
{
	my $self = shift;
	my ($mesh, $side, $loop) = @_;
	my $clone = $self->clone();
	$mesh->CopyVertices($clone);
	for (my $i = 0; $i < $clone->Vertices-2; $i++)
	{
		next if $clone->Vertice(0)->Position == $clone->Vertice($i+1)->Position;
		next if $clone->Vertice(0)->Position == $clone->Vertice($i+2)->Position;
		next if $clone->Vertice($i+1)->Position == $clone->Vertice($i+2)->Position;
		if (!$side || ($side & 2) == 2)
		{
			$mesh->AddFace(Triangle->new(
				$clone->Vertice(0)->clone,
				$clone->Vertice($i+1)->clone,
				$clone->Vertice($i+2)->clone,
			));
		}
		if ($side && $side == 1)
		{
			$mesh->AddFace(Triangle->new(
				$clone->Vertice(0)->clone,
				$clone->Vertice($i+2)->clone,
				$clone->Vertice($i+1)->clone,
			));
			#if ($i != 2)
			;#->SetUV(0, 0, 1, 1);
		}
	}
	#if ($loop)
	#{
	#	if (!$side || ($side & 2) == 2)
	#	{
	#		$loop->AddFace(Triangle->new(
	#			$clone->Vertice(0),
	#			$clone->Vertice($i+1),
	#			$clone->Vertice($i+2),
	#		));
	#	}
	#	if ($side && $side == 1)
	#	{
	#		$loop->AddFace(Triangle->new(
	#			$clone->Vertice(0),
	#			$clone->Vertice($i+2),
	#			$clone->Vertice($i+1),
	#		));
	#		#if ($i != 2)
	#		;#->SetUV(0, 0, 1, 1);
	#	}
	#}
	return $clone;
}

# We know that if we extrude a circle, we can
# create squares for every new pair of vertices
sub Extrude
{
	my $self = shift;
	my ($mesh, $side, $loop) = @_;
	$self = $self->clone();
	$mesh->CopyVertices($self);
	my $clone = $self->clone();
	$mesh->CopyVertices($clone);
	for (my $i = 1; $i < $clone->Vertices; $i++)
	{
		if (!$side || ($side & 2) == 2)
		{
			$mesh->AddQuad(
				$self->Vertice($i-1),
				$self->Vertice($i),
				$clone->Vertice($i),
				$clone->Vertice($i-1),
			);#->SetUV(0, 0, 1, 1);
		}
		if ($side && $side == 1)
		{
			$mesh->AddQuad(
				$self->Vertice($i-1),
				$clone->Vertice($i-1),
				$clone->Vertice($i),
				$self->Vertice($i),
			)
			#if ($i != 2)
			;#->SetUV(0, 0, 1, 1);
		}
	}
	if ($loop)
	{
		if (!$side || ($side & 2) == 2)
		{
			$loop->AddQuad(
				$self->Vertice(-1),
				$self->Vertice(0),
				$clone->Vertice(0),
				$clone->Vertice(-1),
			);# ->SetUV(0, 0, 1, 1);
		}
		if ($side && $side == 1)
		{
			$loop->AddQuad(
				$self->Vertice(-1),
				$clone->Vertice(-1),
				$clone->Vertice(0),
				$self->Vertice(0),
			); #->SetUV(0, 0, 1, 1);
		}
	}
	return $clone;
}

1;